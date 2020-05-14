using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Bader.Edge.ModuleHost;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AzureDeviceClient = Microsoft.Azure.Devices.Client;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// The module host builder extensions.
    /// </summary>
    public static class ModuleHostBuilderExtensions
    {
        /// <summary>
        /// Uses the given type as the start up class for the edge module.
        /// </summary>
        /// <typeparam name="TStartup">The type of the start up class.</typeparam>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="waitForDebugger">Whether the module host should wait until a debugger is attached.</param>
        /// <returns>The host builder.</returns>
        public static IHostBuilder UseStartup<TStartup>(this IHostBuilder hostBuilder, bool waitForDebugger = false) where TStartup : class
        {
            if (waitForDebugger)
            {
                while (!Debugger.IsAttached)
                {
                    Thread.Sleep(100);
                }
            }

            var startup = typeof(IStartup).IsAssignableFrom(typeof(TStartup))
                ? Activator.CreateInstance<TStartup>() as IStartup
                : new ConventionalStartup(typeof(TStartup));

            if (startup is null)
            {
                throw new InvalidOperationException("Could not create startup implementation");
            }

            return hostBuilder.UseStartup(startup);
        }

        /// <summary>
        /// Uses the given instance as the start up class for the edge module.
        /// </summary>
        /// <param name="hostBuilder">The host builder.</param>
        /// <param name="startup">The instance of the start up class.</param>
        /// <returns>The host builder.</returns>
        internal static IHostBuilder UseStartup(this IHostBuilder hostBuilder, IStartup startup)
        {
            _ = startup ?? throw new ArgumentNullException(nameof(startup));

            return hostBuilder.ConfigureServices(serviceCollection =>
            {
                startup.ConfigureServices(serviceCollection);

                serviceCollection.AddSingleton(_ => startup);

                if (!serviceCollection.Any(p => p.ServiceType == typeof(IModuleClient)))
                {
                    serviceCollection.AddSingleton(CreateModuleClient);
                }
            });
        }

        private static IModuleClient CreateModuleClient(IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetServices<AzureDeviceClient.ITransportSettings>().ToArray();
            if ((settings?.Length ?? 0) == 0)
            {
                var mqttSetting = new MqttTransportSettings(AzureDeviceClient.TransportType.Mqtt_Tcp_Only);
                settings = new[] { mqttSetting };

                var logger = serviceProvider.GetService<ILogger<ModuleHostBuilder>>();
                logger.LogInformation($"No transport settings found, using MQTT over TCP.");
            }

            var azureModuleClient = AzureDeviceClient.ModuleClient.CreateFromEnvironmentAsync(settings)
                .GetAwaiter().GetResult();
            return new ModuleClient(azureModuleClient);
        }
    }
}
