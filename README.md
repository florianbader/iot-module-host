# IoT Edge Module Generic Host

[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Bader.Edge.ModuleHost)](https://www.nuget.org/packages/Bader.Edge.ModuleHost/)
[![Build status](https://dev.azure.com/ait-fb/Public/_apis/build/status/IoT/iot-module-host.NuGet)](https://dev.azure.com/ait-fb/Public/_build/latest?definitionId=52?branchName=master)

IoT Module Host is a generic host implementation for IoT Edge modules. It makes it easier to start a new module implementation by taking away the boilerplate code which is required.

The solution consists of the following features:

* [Startup class](#Startup)
  * dependency injection
  * configuration of module client
  * desired property update
  * connection status change
* Define message handlers in classes
* Define method handlers in classes
* Define async timers for periodic tasks
* Throttled event processor for efficient message usage 
* Module client wrapper with interface to make testing easier


## Getting Started

### Prerequisites
[.NET Core 3.1+ SDK](https://www.microsoft.com/net/download/core) must be installed.

### Installing
Install the dotnet-script CLI tool: ``dotnet tool restore``  
Build the solution ``dotnet build``

## Features

## Startup

```cs
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
```

## Sample
See the [sample solution](samples/starter) to see how it works.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/FlorianBader/iot-module-host/tags).

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
