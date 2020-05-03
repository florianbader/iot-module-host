using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Bader.Edge.ModuleHost.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Bader.Edge.ModuleHost
{
    /// <summary>
    /// The module client hosted service which starts the edge module.
    /// </summary>
    internal class ModuleClientHostedService : IHostedService
    {
        private readonly ILogger<ModuleClientHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IModuleClient _moduleClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleClientHostedService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="logger">The logger.</param>
        public ModuleClientHostedService(IServiceProvider serviceProvider, ILogger<ModuleClientHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _moduleClient = new ModuleClient(null); // moduleClient can never be null
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _moduleClient = _serviceProvider.GetService<IModuleClient>();
                if (_moduleClient is null)
                {
                    throw new InvalidOperationException("IModuleClient not found.");
                }

                await _moduleClient.OpenAsync().ConfigureAwait(false);

                _logger.LogInformation("IoT Hub module client initialized.");

                await ConfigureStartupAsync().ConfigureAwait(false);

                await ConfigureMethodsAsync().ConfigureAwait(false);

                await ConfigureMessagesAsync().ConfigureAwait(false);
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

        private async Task ConfigureMessagesAsync()
        {
            var messageHandlerTypes = GetAssemblyTypes<IMessageHandler>();

            foreach (var messageHandlerType in messageHandlerTypes)
            {
                var messageHandlerAttribute = GetCustomAttribute<MessageHandlerAttribute>(messageHandlerType);

                var messageHandlerInstance = CreateInstance<IMessageHandler>(messageHandlerType);
                if (messageHandlerInstance == null)
                {
                    _logger.LogWarning($"Message handler for type {messageHandlerType.FullName} could not be created.");
                    continue;
                }

                if (messageHandlerAttribute.IsDefault)
                {
                    _logger.LogTrace($"Registering message handler {messageHandlerType.FullName} as default...");
                    await _moduleClient.SetMessageHandlerAsync((message, _) => messageHandlerInstance.HandleMessageAsync(message), null!).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogTrace($"Registering message handler {messageHandlerType.FullName} for input {messageHandlerAttribute.InputName}...");
                    await _moduleClient.SetInputMessageHandlerAsync(messageHandlerAttribute.InputName,
                        (message, _) => messageHandlerInstance.HandleMessageAsync(message), null!).ConfigureAwait(false);
                }
            }
        }

        private async Task ConfigureMethodsAsync()
        {
            var methodHandlerTypes = GetAssemblyTypes<IMethodHandler>();

            foreach (var methodHandlerType in methodHandlerTypes)
            {
                var methodHandlerAttribute = GetCustomAttribute<MethodHandlerAttribute>(methodHandlerType);

                var methodHandlerInstance = CreateInstance<IMethodHandler>(methodHandlerType);
                if (methodHandlerInstance == null)
                {
                    _logger.LogWarning($"Method handler for type {methodHandlerType.FullName} could not be created.");
                    continue;
                }

                if (methodHandlerAttribute.IsDefault)
                {
                    _logger.LogTrace($"Registering method handler {methodHandlerType.FullName} as default...");
                    await _moduleClient.SetMethodDefaultHandlerAsync((methodRequest, _) => methodHandlerInstance.HandleMethodAsync(methodRequest), null!).ConfigureAwait(false);
                }
                else
                {
                    _logger.LogTrace($"Registering method handler {methodHandlerType.FullName} for method {methodHandlerAttribute.MethodName}...");
                    await _moduleClient.SetMethodHandlerAsync(methodHandlerAttribute.MethodName,
                        (message, _) => methodHandlerInstance.HandleMethodAsync(message), null!).ConfigureAwait(false);
                }
            }
        }

        private async Task ConfigureStartupAsync()
        {
            var startup = _serviceProvider.GetService<IStartup>();
            if (startup == null)
            {
                _logger.LogWarning("No startup implementation found.");
                return;
            }

            await startup.ConfigureAsync(_moduleClient).ConfigureAwait(false);

            await _moduleClient.SetDesiredPropertyUpdateCallbackAsync((desiredProperties, _) =>
                startup.DesiredPropertyUpdateAsync(desiredProperties), null!).ConfigureAwait(false);

            _moduleClient.SetConnectionStatusChangesHandler((status, reason) =>
                _ = startup.ConnectionStatusChangesAsync(status, reason));
        }

        private T? CreateInstance<T>(Type type) where T : class
            => Activator.CreateInstance(
                type,
                type.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .First()
                .GetParameters()
                .Select(p => _serviceProvider.GetRequiredService(p.ParameterType))
                .ToArray()) as T;

        private TAttribute GetCustomAttribute<TAttribute>(Type type) where TAttribute : Attribute =>
            type.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault() as TAttribute
                ?? Activator.CreateInstance<TAttribute>();
    }
}
