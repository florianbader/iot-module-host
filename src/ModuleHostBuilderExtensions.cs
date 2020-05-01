using System;
using System.Collections.Generic;
using System.Linq;
using Bader.Edge.ModuleHost;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Extensions.Configuration;
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
        {
            var startup = typeof(IStartup).IsAssignableFrom(typeof(TStartup))
                ? Activator.CreateInstance<TStartup>() as IStartup
                : new ConventionalStartup(typeof(TStartup));

            if (startup == null)
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
            if (startup == null)
            {
                throw new ArgumentNullException(nameof(startup));
            }

            hostBuilder.ConfigureServices(s => startup.ConfigureServices(s));

            hostBuilder.ConfigureServices(s => s.AddSingleton(_ => startup));

            hostBuilder.ConfigureServices(s =>
            {
                if (!s.Any(p => p.ServiceType == typeof(IModuleClient)))
                {
                    s.AddSingleton(CreateModuleClient);
                }
            });

            return hostBuilder;
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
