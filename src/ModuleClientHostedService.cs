using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AzureModuleClient = Microsoft.Azure.Devices.Client.ModuleClient;

namespace AIT.Devices
{
    internal class ModuleClientHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ModuleClientHostedService>? _logger;
        private IModuleClient? _moduleClient;

        public ModuleClientHostedService(IServiceProvider serviceProvider, ILogger<ModuleClientHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                _moduleClient = _serviceProvider.GetService<IModuleClient>();
                if (_moduleClient == null)
                {
                    throw new InvalidOperationException("IModuleClient not found.");
                }

                await _moduleClient.OpenAsync();

                _logger?.LogInformation("IoT Hub module client initialized.");

                await ConfigureStartupAsync();

                await ConfigureMethodsAsync();

                await ConfigureMessagesAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred on startup");
                throw;
            }
        }

        private async Task ConfigureMethodsAsync()
        {
            var methodHandlerTypes = GetAssemblyTypes<IMethodHandler>();

            foreach (var methodHandlerType in methodHandlerTypes)
            {
                var methodHandlerAttribute = GetCustomAttribute<MethodHandlerAttribute>(methodHandlerType);

                // TODO: Use service provider to inject constructor parameters
                var methodHandlerInstance = Activator.CreateInstance(methodHandlerType, new[] { _moduleClient }) as IMethodHandler;
                if (methodHandlerInstance == null)
                {
                    _logger?.LogWarning($"Method handler for type {methodHandlerType.FullName} could not be created.");
                    continue;
                }

                if (methodHandlerAttribute.IsDefault)
                {
                    _logger?.LogTrace($"Registering method handler {methodHandlerType.FullName} as default...");
                    await _moduleClient!.SetMethodDefaultHandlerAsync((methodRequest, _) => methodHandlerInstance.HandleMethodAsync(methodRequest), null!);
                }
                else
                {
                    _logger?.LogTrace($"Registering method handler {methodHandlerType.FullName} for method {methodHandlerAttribute.MethodName}...");
                    await _moduleClient!.SetMethodHandlerAsync(methodHandlerAttribute.MethodName,
                        (message, _) => methodHandlerInstance.HandleMethodAsync(message), null!);
                }
            }
        }

        private async Task ConfigureStartupAsync()
        {
            var startup = _serviceProvider.GetService<IStartup>();
            if (startup == null)
            {
                _logger?.LogWarning("No startup implementation found.");
                return;
            }

            await startup.ConfigureAsync(_moduleClient!);

            await _moduleClient!.SetDesiredPropertyUpdateCallbackAsync((desiredProperties, _) =>
                startup.DesiredPropertyUpdateAsync(desiredProperties), null!);

            _moduleClient!.SetConnectionStatusChangesHandler((status, reason) =>
                _ = startup.ConnectionStatusChangesAsync(status, reason));
        }

        private async Task ConfigureMessagesAsync()
        {
            var messageHandlerTypes = GetAssemblyTypes<IMessageHandler>();

            foreach (var messageHandlerType in messageHandlerTypes)
            {
                var messageHandlerAttribute = GetCustomAttribute<MessageHandlerAttribute>(messageHandlerType);

                // TODO: Use service provider to inject constructor parameters
                var messageHandlerInstance = Activator.CreateInstance(messageHandlerType, new[] { _moduleClient }) as IMessageHandler;
                if (messageHandlerInstance == null)
                {
                    _logger?.LogWarning($"Message handler for type {messageHandlerType.FullName} could not be created.");
                    continue;
                }

                if (messageHandlerAttribute.IsDefault)
                {
                    _logger?.LogTrace($"Registering message handler {messageHandlerType.FullName} as default...");
                    await _moduleClient!.SetMessageHandlerAsync((message, _) => messageHandlerInstance.HandleMessageAsync(message), null!);
                }
                else
                {
                    _logger?.LogTrace($"Registering message handler {messageHandlerType.FullName} for input {messageHandlerAttribute.InputName}...");
                    await _moduleClient!.SetInputMessageHandlerAsync(messageHandlerAttribute.InputName,
                        (message, _) => messageHandlerInstance.HandleMessageAsync(message), null!);
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _moduleClient!.CloseAsync();
        }

        private static IEnumerable<Type> GetAssemblyTypes<T>()
        {
            // TODO: Get types from configured assemblies
            return Assembly.GetEntryAssembly()
                .GetModules()
                .SelectMany(m => m.GetTypes())
                .Where(t => !t.IsAbstract && typeof(T).IsAssignableFrom(t));
        }

        private TAttribute GetCustomAttribute<TAttribute>(Type type) where TAttribute : Attribute =>
            type.GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault() as TAttribute
                ?? Activator.CreateInstance<TAttribute>() as TAttribute;
    }
}
