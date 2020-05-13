using System;
using System.Linq;
using Bader.Edge.ModuleHost;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AzureDeviceClient = Microsoft.Azure.Devices.Client;

namespace Microsoft.Extensions.Hosting
{
    public static class ModuleHostBuilderExtensions
    {
        /// <summary>
        /// Uses the given type as the start up class for the edge module.
        /// </summary>
        /// <typeparam name="TStartup">The type of the start up class.</typeparam>
        /// <param name="hostBuilder">The host builder.</param>
        /// <returns>The host builder.</returns>
        public static IHostBuilder UseStartup<TStartup>(this IHostBuilder hostBuilder) where TStartup : class
            => hostBuilder.ConfigureServices(serviceCollection =>
            {
                startup.ConfigureServices(serviceCollection);

                serviceCollection.AddSingleton<IStartup>(typeof(TStartup));

                if (!serviceCollection.Any(p => p.ServiceType == typeof(IModuleClient)))
                {
                    serviceCollection.AddSingleton(CreateModuleClient);
                }
            });

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
