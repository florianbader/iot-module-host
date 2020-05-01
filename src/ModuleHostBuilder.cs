using System;
using System.Collections.Generic;
using Bader.Edge.ModuleHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// The module host builder.
    /// </summary>
    public class ModuleHostBuilder : IHostBuilder
    {
        private readonly IHostBuilder _hostBuilder;

        /// <inheritdoc />
        public IDictionary<object, object> Properties => new Dictionary<object, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleHostBuilder"/> class.
        /// </summary>
        public ModuleHostBuilder() : this(null!)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleHostBuilder"/> class.
        /// </summary>
        /// <param name="args">The arguments for the default builder.</param>
        public ModuleHostBuilder(string[] args) =>
            _hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureServices(s => s.AddHostedService<ModuleClientHostedService>())
                .UseConsoleLifetime();

        /// <inheritdoc />
        public IHost Build() => _hostBuilder.Build();

        /// <inheritdoc />
        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) =>
            _hostBuilder.ConfigureAppConfiguration(configureDelegate);

        /// <inheritdoc />
        public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate) =>
            _hostBuilder.ConfigureContainer(configureDelegate);

        /// <inheritdoc />
        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) =>
            _hostBuilder.ConfigureHostConfiguration(configureDelegate);

        /// <inheritdoc />
        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) =>
            _hostBuilder.ConfigureServices(configureDelegate);

        /// <inheritdoc />
        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) =>
            _hostBuilder.UseServiceProviderFactory(factory);

        /// <inheritdoc />
        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) =>
            _hostBuilder.UseServiceProviderFactory(factory);
    }
}
