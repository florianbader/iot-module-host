using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AIT.Devices
{
    internal class ConventionalStartup : IStartup
    {
        private readonly Type _type;
        private readonly object _instance;

        public ConventionalStartup(Type type)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _instance = Activator.CreateInstance(type);
        }

        public Task ConfigureAsync(IModuleClient moduleClient) =>
            _type.GetMethod(nameof(ConfigureAsync))?.Invoke(_instance, new object[] { moduleClient }) as Task ?? Task.CompletedTask;

        public void ConfigureServices(IServiceCollection services) =>
            _type.GetMethod(nameof(ConfigureServices))?.Invoke(_instance, new object[] { services });

        public Task ConnectionStatusChangesAsync(ConnectionStatus status, ConnectionStatusChangeReason reason) =>
            _type.GetMethod(nameof(ConnectionStatusChangesAsync))?.Invoke(_instance, new object[] { status, reason }) as Task ?? Task.CompletedTask;

        public Task DesiredPropertyUpdateAsync(TwinCollection desiredProperties) =>
            _type.GetMethod(nameof(DesiredPropertyUpdateAsync))?.Invoke(_instance, new object[] { desiredProperties }) as Task ?? Task.CompletedTask;
    }
}