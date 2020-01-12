using AIT.Devices;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using AzureDeviceClient = Microsoft.Azure.Devices.Client;

namespace Microsoft.Extensions.Hosting
{
    public class ModuleHostBuilder : IHostBuilder
    {
        private readonly IHostBuilder _hostBuilder;

        public IDictionary<object, object> Properties => new Dictionary<object, object>();

        public ModuleHostBuilder() : this(null!)
        {
        }

        public ModuleHostBuilder(string[] args)
        {
            _hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureServices(s => s.AddHostedService<ModuleClientHostedService>())
                .UseConsoleLifetime();
        }

        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) =>
            _hostBuilder.ConfigureAppConfiguration(configureDelegate);

        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) =>
            _hostBuilder.ConfigureServices(configureDelegate);

        public IHost Build() => _hostBuilder.Build();

        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) =>
            _hostBuilder.ConfigureHostConfiguration(configureDelegate);

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) =>
            _hostBuilder.UseServiceProviderFactory(factory);

        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) =>
            _hostBuilder.UseServiceProviderFactory(factory);

        public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate) =>
            _hostBuilder.ConfigureContainer(configureDelegate);
    }

    public static class ModuleHostBuilderExtensions
    {
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

        internal static IHostBuilder UseStartup(this IHostBuilder hostBuilder, IStartup startup)
        {
            if (startup == null) throw new ArgumentNullException(nameof(startup));

            hostBuilder.ConfigureServices(s => startup.ConfigureServices(s));

            hostBuilder.ConfigureServices(s => s.AddSingleton<IStartup>(_ => startup));

            hostBuilder.ConfigureServices(s =>
            {
                if (!s.Any(p => p.ServiceType == typeof(IModuleClient)))
                {
                    s.AddSingleton<IModuleClient>(CreateModuleClient);
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

            var azureModuleClient = AzureDeviceClient.ModuleClient.CreateFromEnvironmentAsync()
                .GetAwaiter().GetResult();
            return new ModuleClient(azureModuleClient);
        }
    }
}
