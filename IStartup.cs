using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace AIT.Devices
{
    public interface IStartup
    {
        void ConfigureServices(IServiceCollection services);

        Task ConfigureAsync(IModuleClient moduleClient);

        Task DesiredPropertyUpdateAsync(TwinCollection desiredProperties);

        Task ConnectionStatusChangesAsync(ConnectionStatus status, ConnectionStatusChangeReason reason);
    }
}