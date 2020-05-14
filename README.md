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
* [Define message handlers in classes](#Message-Handlers)
* [Define method handlers in classes](#Method-Handlers)
* [Define async timers for periodic tasks](#Async-Timers-/-Throttled-Event-Processor)
* [Throttled event processor for efficient message usage](#Async-Timers-/-Throttled-Event-Processor)
* [Module client wrapper with interface to make testing easier](#Module-Client-Mock)


## Getting Started

### Prerequisites
[.NET Core 3.1+ SDK](https://www.microsoft.com/net/download/core) must be installed.

### Installing
Install the dotnet-script CLI tool:  
``dotnet tool restore``

Build the solution:  
``dotnet build``

## Features

### Startup

```csharp
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

### Message Handlers

```csharp
[MessageHandler("input1")]
public class Input1MessageHandler : MessageHandlerBase
{
    private readonly IModuleClient _moduleClient;
    private readonly ILogger<Input1MessageHandler> _logger;

    public Input1MessageHandler(IModuleClient moduleClient, ILogger<Input1MessageHandler> logger) : base(logger)
    {
        _moduleClient = moduleClient;
        _logger = logger;
    }

    protected override async Task<MessageResponse> HandleMessageAsync(Message message)
    {
        message.Properties["original-connection-module-id"] = message.ConnectionModuleId;

        await _moduleClient.SendEventAsync(message).ConfigureAwait(false);

        var json = await JsonDocument.ParseAsync(message.BodyStream).ConfigureAwait(false);
        _logger.LogInformation("Received message: {Message}", json.RootElement.ToString());

        return Ok();
    }
}
```

### Method Handlers

```csharp
[MethodHandler("method1")]
public class Method1MethodHandler : MethodHandlerBase
{
    private readonly IModuleClient _moduleClient;
    private readonly ILogger<Method1MethodHandler> _logger;

    public Method1MethodHandler(IModuleClient moduleClient, ILogger<Method1MethodHandler> logger) : base(logger)
    {
        _moduleClient = moduleClient;
        _logger = logger;
    }

    protected override async Task<MethodResponse> HandleMethodAsync(MethodRequest methodRequest)
    {
        var json = JsonDocument.Parse(methodRequest.DataAsJson);
        _logger.LogInformation("Received method call '{MethodName}' with payload: {Payload}",
            methodRequest.Name, json.RootElement.ToString());

        return Ok();
    }
}
```

### Async Timers / Throttled Event Processor

```csharp
public class MessageReader : HostedTimerService
{
    private readonly ThrottledEventProcessor _throttledEventProcessor;

    public MessageReader(IModuleClient moduleClient, ILogger<MessageReader> logger)
        : base(interval: TimeSpan.FromSeconds(60), shouldCallInitially: true, shouldWaitForElapsedToComplete: true)
    {
        // max capacity = 1000, timeout = 60 seconds 
        _throttledEventProcessor = new ThrottledEventProcessor(moduleClient, 1000, TimeSpan.FromSeconds(60), logger);
        _throttledEventProcessor.Start();
    }

    protected override async Task ElapsedAsync(CancellationToken cancellationToken)
    {
        var data = await ReadDataFromSomewhereAsync();

        var message = MessageFactory.CreateMessage(data);
        _throttledEventProcessor.EnqueueEvent(message);
    }

    private Task<IEnumerable<object>> ReadDataFromSomewhereAsync() => Task.FromResult(Enumerable.Empty<object>());
}
```

### Module Client Mock

```csharp
var moduleClientMock = new Mock<IModuleClient>();
// [...]
moduleClientMock.Verify(m => m.SendEventAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
```

## Sample
See the [sample solution](samples/starter) to see how it works.

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/FlorianBader/iot-module-host/tags).

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
