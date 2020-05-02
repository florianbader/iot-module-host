using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Bader.Edge.ModuleHost
{
    /// <summary>
    /// The conventional startup which is used if the user did not provide a class which implements the IStartup interface.
    /// </summary>
    internal class ConventionalStartup : IStartup
    {
        private readonly object _instance;
        private readonly Type _type;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConventionalStartup"/> class.
        /// </summary>
        /// <param name="type">The type of the user startup class.</param>
        public ConventionalStartup(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _instance = Activator.CreateInstance(type);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConventionalStartup"/> class.
        /// </summary>
        /// <param name="type">The type of the user startup class.</param>
        /// <param name="instance">The instance of the user startup class.</param>
        internal ConventionalStartup(Type type, object instance)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
        }

        /// <inheritdoc />
        public Task ConfigureAsync(IModuleClient moduleClient) =>
            _type.GetMethod(nameof(ConfigureAsync))?.Invoke(_instance, new object[] { moduleClient }) as Task ?? Task.CompletedTask;

        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services) =>
            _type.GetMethod(nameof(ConfigureServices))?.Invoke(_instance, new object[] { services });

        /// <inheritdoc />
        public Task ConnectionStatusChangesAsync(ConnectionStatus status, ConnectionStatusChangeReason reason) =>
            _type.GetMethod(nameof(ConnectionStatusChangesAsync))?.Invoke(_instance, new object[] { status, reason }) as Task ?? Task.CompletedTask;

        /// <inheritdoc />
        public Task DesiredPropertyUpdateAsync(TwinCollection desiredProperties) =>
            _type.GetMethod(nameof(DesiredPropertyUpdateAsync))?.Invoke(_instance, new object[] { desiredProperties }) as Task ?? Task.CompletedTask;
    }
}
