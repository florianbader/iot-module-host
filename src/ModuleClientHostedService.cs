using System.Reflection;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Bader.Edge.ModuleHost.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Bader.Edge.ModuleHost;

/// <summary>
/// The module client hosted service which starts the edge module.
/// </summary>
internal sealed class ModuleClientHostedService : IHostedService, IDisposable
{
    private readonly ICollection<Func<TwinCollection, Task>> _desiredPropertiesChangedCallbacks = new List<Func<TwinCollection, Task>>();
    private readonly SemaphoreSlim _desiredPropertyUpdateSemaphore = new(1, 1);
    private readonly ILogger<ModuleClientHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private bool _disposed;
    private long _lastDesiredPropertiesVersion = 0;
    private IModuleClient _moduleClient = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModuleClientHostedService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public ModuleClientHostedService(IServiceProvider serviceProvider, ILogger<ModuleClientHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _desiredPropertyUpdateSemaphore.Dispose();
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _moduleClient = _serviceProvider.GetService<IModuleClient>() ?? throw new InvalidOperationException("Module client not found.");

            await _moduleClient.OpenAsync().ConfigureAwait(false);

            _logger.LogInformation("IoT Hub module client initialized.");

            await ConfigureStartupAsync().ConfigureAwait(false);

            await ConfigureMethodsAsync().ConfigureAwait(false);

            await ConfigureMessagesAsync().ConfigureAwait(false);

            await ConfigureDesiredPropertiesChangedAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred on startup");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken) => await _moduleClient.CloseAsync().ConfigureAwait(false);

    private static IEnumerable<Type> GetAssemblyTypes<T>() =>
        // TODO: Get types from configured assemblies
        Assembly.GetEntryAssembly()
        .GetModules()
        .SelectMany(m => m.GetTypes())
        .Where(t => !t.IsAbstract && typeof(T).IsAssignableFrom(t));

    private async Task ConfigureDesiredPropertiesChangedAsync()
    {
        var desiredPropertiesChangedHandlerTypes = GetAssemblyTypes<IMessageHandler>();

        foreach (var desiredPropertiesChangedHandlerType in desiredPropertiesChangedHandlerTypes)
        {
            var desiredPropertiesChangedHandlerAttribute = GetCustomAttribute<DesiredPropertiesChangedHandlerAttribute>(desiredPropertiesChangedHandlerType);

            _logger.LogTrace("Registering desired properties changed handler {DesiredPropertiesChangedHandlerType}...", desiredPropertiesChangedHandlerType.FullName);
            _desiredPropertiesChangedCallbacks.Add(CreateDesiredPropertiesChangedCallback(desiredPropertiesChangedHandlerType, desiredPropertiesChangedHandlerAttribute));
        }

        await _moduleClient.SetDesiredPropertyUpdateCallbackAsync((twinCollection, _) => DesiredPropertyUpdateCallback(twinCollection), null!).ConfigureAwait(false);
    }

    private async Task ConfigureMessagesAsync()
    {
        var messageHandlerTypes = GetAssemblyTypes<IMessageHandler>();
        var messageHandlerTypesByName = messageHandlerTypes.GroupBy(type =>
        {
            var messageHandlerAttribute = GetCustomAttribute<MessageHandlerAttribute>(type);
            return messageHandlerAttribute.InputName;
        });

        foreach (var messageHandlerTypesByNamePair in messageHandlerTypesByName)
        {
            var inputName = messageHandlerTypesByNamePair.Key;
            var inputMessageHandlerTypes = messageHandlerTypesByNamePair.ToArray();

            var messageHandlerFunc = CreateMessageHandlerCallback(inputMessageHandlerTypes);

            if (string.IsNullOrEmpty(inputName))
            {
                _logger.LogTrace("Registering message handlers {MethodHandlerType} as default...", inputMessageHandlerTypes);
                await _moduleClient.SetMessageHandlerAsync((message, _) => messageHandlerFunc(message), null!).ConfigureAwait(false);
            }
            else
            {
                _logger.LogTrace("Registering message handlers {MethodHandlerType} for input {inputName}...", inputMessageHandlerTypes, inputName);
                await _moduleClient.SetInputMessageHandlerAsync(inputName,
                    (message, _) => messageHandlerFunc(message), null!).ConfigureAwait(false);
            }
        }
    }

    private async Task ConfigureMethodsAsync()
    {
        var methodHandlerTypes = GetAssemblyTypes<IMethodHandler>();

        foreach (var methodHandlerType in methodHandlerTypes)
        {
            var methodHandlerAttribute = GetCustomAttribute<MethodHandlerAttribute>(methodHandlerType);

            var methodHandlerFunc = CreateMethodHandlerCallback(methodHandlerType, methodHandlerAttribute);

            if (methodHandlerAttribute.IsDefault)
            {
                _logger.LogTrace("Registering method handler {MethodHandlerType} as default...", methodHandlerType.FullName);
                await _moduleClient.SetMethodDefaultHandlerAsync((methodRequest, _) => methodHandlerFunc(methodRequest), null!).ConfigureAwait(false);
            }
            else
            {
                _logger.LogTrace("Registering method handler {MethodHandlerType} for method {MethodName}...", methodHandlerType.FullName, methodHandlerAttribute.MethodName);
                await _moduleClient.SetMethodHandlerAsync(methodHandlerAttribute.MethodName,
                    (message, _) => methodHandlerFunc(message), null!).ConfigureAwait(false);
            }
        }
    }

    private async Task ConfigureStartupAsync()
    {
        var startup = _serviceProvider.GetService<IStartup>();
        if (startup is null)
        {
            _logger.LogWarning("No startup implementation found.");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        await startup.InitializeAsync(scope.ServiceProvider).ConfigureAwait(false);

        _desiredPropertiesChangedCallbacks.Add(async (desiredProperties) =>
        {
            using var scope = _serviceProvider.CreateScope();
            await startup.DesiredPropertyUpdateAsync(scope.ServiceProvider, desiredProperties);
        });

        _moduleClient.SetConnectionStatusChangesHandler(async (status, reason) =>
        {
            using var scope = _serviceProvider.CreateScope();
            await startup.ConnectionStatusChangesAsync(scope.ServiceProvider, status, reason);
        });
    }

    private Func<TwinCollection, Task> CreateDesiredPropertiesChangedCallback(Type type, DesiredPropertiesChangedHandlerAttribute attribute) => async (desiredProperties) =>
    {
        if (!desiredProperties.Contains(attribute.PropertyName))
        {
            _logger.LogTrace("Skipping desired properties changed handler {HandlerName} because property {PropertyName} was not found.", type.FullName, attribute.PropertyName);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        using var cancellationTokenSource = new CancellationTokenSource(attribute.Timeout);
        if (scope.ServiceProvider.GetRequiredService(type) is not IDesiredPropertiesChangedHandler desiredPropertiesChangedHandlerInstance)
        {
            throw new InvalidOperationException($"Desired properties changed handler for type {type.FullName} could not be created.");
        }

        var desiredPropertiesParameter = desiredProperties[attribute.PropertyName] is JToken token && token.Type != JTokenType.Null ? token : null;
        await desiredPropertiesChangedHandlerInstance.HandleDesiredPropertiesChangedAsync(desiredPropertiesParameter, cancellationTokenSource.Token);
    };

    private Func<Message, Task<MessageResponse>> CreateMessageHandlerCallback(IEnumerable<Type> inputMessageHandlerTypes)
    {
        var messageHandlerFunc = new Func<Message, Task<MessageResponse>>(async (message) =>
        {
            using var scope = _serviceProvider.CreateScope();

            var messageResponse = MessageResponse.Completed;
            foreach (var inputMessageHandlerType in inputMessageHandlerTypes)
            {
                try
                {
                    var messageHandlerAttribute = GetCustomAttribute<MessageHandlerAttribute>(inputMessageHandlerType);

                    using var cancellationTokenSource = new CancellationTokenSource(messageHandlerAttribute.Timeout);
                    if (scope.ServiceProvider.GetRequiredService(inputMessageHandlerType) is not IMessageHandler messageHandlerInstance)
                    {
                        throw new InvalidOperationException($"Method handler for type {inputMessageHandlerType.FullName} could not be created.");
                    }

                    messageResponse = await messageHandlerInstance.HandleMessageAsync(message, cancellationTokenSource.Token);
                }
                catch
                {
                    // swallow exception because message handler needs to deal with it
                }
            }

            return messageResponse;
        });
        return messageHandlerFunc;
    }

    private Func<MethodRequest, Task<MethodResponse>> CreateMethodHandlerCallback(Type methodHandlerType, MethodHandlerAttribute methodHandlerAttribute)
        => new Func<MethodRequest, Task<MethodResponse>>(async (methodRequest) =>
        {
            using var cancellationTokenSource = new CancellationTokenSource(methodHandlerAttribute.Timeout);
            using var scope = _serviceProvider.CreateScope();

            if (scope.ServiceProvider.GetRequiredService(methodHandlerType) is not IMethodHandler methodHandlerInstance)
            {
                throw new InvalidOperationException($"Method handler for type {methodHandlerType.FullName} could not be created.");
            }

            return await methodHandlerInstance.HandleMethodAsync(methodRequest, cancellationTokenSource.Token);
        });

    private async Task DesiredPropertyUpdateCallback(TwinCollection desiredProperties)
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await _desiredPropertyUpdateSemaphore.WaitAsync(cancellationTokenSource.Token);

        try
        {
            if (_lastDesiredPropertiesVersion > desiredProperties.Version)
            {
                // if we get an old update we need to get the twin in order to have the latest version of it because desired property updates are incremental
                _logger.LogTrace("Getting module twin because last desired properties change was older than the update.");

                var twin = await _moduleClient.GetTwinAsync(cancellationTokenSource.Token);
                desiredProperties = twin.Properties.Desired;
            }

            foreach (var desiredPropertyChangedCallback in _desiredPropertiesChangedCallbacks)
            {
                try
                {
                    await desiredPropertyChangedCallback(desiredProperties);
                }
                catch
                {
                    // swallow exception because desired properties changed handler needs to deal with it
                }
            }
        }
        finally
        {
            _desiredPropertyUpdateSemaphore.Release();

            Interlocked.Exchange(ref _lastDesiredPropertiesVersion, desiredProperties.Version);
        }
    }

    private TAttribute GetCustomAttribute<TAttribute>(Type type) where TAttribute : Attribute =>
        type.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault() as TAttribute
            ?? Activator.CreateInstance<TAttribute>();
}
