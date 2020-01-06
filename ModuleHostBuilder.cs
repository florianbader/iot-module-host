using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace AIT.Devices
{
    public class ModuleHostBuilder : IHostBuilder
    {
        private readonly IHostBuilder _hostBuilder;

        public IDictionary<object, object> Properties => new Dictionary<object, object>();

        public ModuleHostBuilder() : this(null)
        {
        }

        public ModuleHostBuilder(string[] args)
        {
            _hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureServices(s =>
                    s.AddHostedService<ModuleClientHostedService>()
                    .AddSingleton<IModuleClient>(new ModuleClient()))
                .UseConsoleLifetime();
        }

        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) =>
            _hostBuilder.ConfigureAppConfiguration(configureDelegate);

        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) =>
            _hostBuilder.ConfigureServices(configureDelegate);

        public IHostBuilder UseStartup<TStartup>() where TStartup : class
        {
            var startup = typeof(IStartup).IsAssignableFrom(typeof(TStartup))
                ? Activator.CreateInstance<TStartup>() as IStartup
                : new ConventionalStartup(typeof(TStartup));

            _hostBuilder.ConfigureServices(s =>
                s.AddSingleton<IStartup>(_ => startup));

            _hostBuilder.ConfigureServices(s => startup.ConfigureServices(s));

            return _hostBuilder;
        }

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
}