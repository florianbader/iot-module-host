using System;
using System.Threading.Tasks;
using AIT.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace starter
{
    // The methods of the Startup class are called by convention and are specified in the IStartup interface.
    // If a method was not found the method is not called.
    public class Startup
    {
        public Task ConfigureAsync(IModuleClient moduleClient)
        {
            // Use the ConfigureAsync method to configure the module client.
            // The module client is not connected yet. Use this method to save the instance of the module client.
            // _moduleClient = moduleClient;

            Log.Information("Configure");
            return Task.CompletedTask;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            // Use the ConfigureServices method to configure services and add new services to the collection.
            // The method is called before the module client is connecting. This can also be used to configure the module client before it connects.

            // 1. Add transport settings which are used by the module client
            // services.AddTransient<ITransportSettings>(_ => new AmqpTransportSettings(TransportType.Amqp_Tcp_Only));
            // services.AddTransient<ITransportSettings>(_ => new MqttTransportSettings(TransportType.Mqtt_Tcp_Only));

            // 2. Set the EdgeHubConnectionString environment variable which is used by the module client.
            // Environment.SetEnvironmentVariable("EdgeHubConnectionString", "HostName=<Host Name>;SharedAccessKeyName=<Key Name>;SharedAccessKey=<SAS Key>")

            Log.Information("ConfigureServices");
        }

        public Task ConnectionStatusChangesAsync(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            Log.Information("ConnectionStatusChangesAsync");
            return Task.CompletedTask;
        }

        public Task DesiredPropertyUpdateAsync(TwinCollection desiredProperties)
        {
            Log.Information("DesiredPropertyUpdateAsync");
            return Task.CompletedTask;
        }
    }
}
