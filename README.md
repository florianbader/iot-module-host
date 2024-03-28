# IoT Edge Module Generic Host

[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/Bader.Edge.ModuleHost)](https://www.nuget.org/packages/Bader.Edge.ModuleHost/)
[![Build status](https://dev.azure.com/ait-fb/Public/_apis/build/status/IoT/iot-module-host.NuGet)](https://dev.azure.com/ait-fb/Public/_build/latest?definitionId=52?branchName=main)

IoT Module Host is a generic host implementation for IoT Edge modules. It makes it easier to start a new module implementation by taking away the boilerplate code which is required.

The solution consists of the following features:

- [IoT Edge Module Generic Host](#iot-edge-module-generic-host)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installing](#installing)
  - [Features](#features)
    - [Startup](#startup)
    - [Message Handlers](#message-handlers)
    - [Method Handlers](#method-handlers)
    - [Desired properties changed Handlers](#desired-properties-changed-handlers)
    - [Async Timers / Batch Event Processor](#async-timers--batch-event-processor)
    - [Module Client Mock](#module-client-mock)
  - [Sample](#sample)
  - [Contributing](#contributing)
  - [Versioning](#versioning)
  - [License](#license)

## Getting Started

### Prerequisites

[.NET 8+ SDK](https://www.microsoft.com/net/download/core) must be installed.

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
    
    public Task InitializeAsync(IServiceProvider serviceProvider)
    {
        // Use the InitializeAsync method to initialize your services and the module client.
        // The module client is not connected yet. Use this method to save the instance of the module client.
        // _moduleClient = serviceProvider.GetRequiredService<IExtendedModuleClient>();

        Log.Information("Configure");
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

    protected override async Task<MessageResponse> HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        message.Properties["original-connection-module-id"] = message.ConnectionModuleId;

        await _moduleClient.SendEventAsync(message).ConfigureAwait(false);

        var json = await JsonDocument.ParseAsync(message.BodyStream, cancellationToken).ConfigureAwait(false);
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

    protected override async Task<MethodResponse> HandleMethodAsync(MethodRequest methodRequest, CancellationToken cancellationToken)
    {
        var json = JsonDocument.Parse(methodRequest.DataAsJson);
        _logger.LogInformation("Received method call '{MethodName}' with payload: {Payload}",
            methodRequest.Name, json.RootElement.ToString());

        return Ok();
    }
}
```

### Desired properties changed Handlers

```csharp
[DesiredPropertiesChangedHandler("configuration")]
public class ConfigurationChangedHandler : DesiredPropertiesChangedHandlerBase
{
    private readonly IModuleClient _moduleClient;
    private readonly ILogger<ConfigurationChangedHandler> _logger;

    public Method1MethodHandler(IModuleClient moduleClient, ILogger<ConfigurationChangedHandler> logger) : base(logger)
    {
        _moduleClient = moduleClient;
        _logger = logger;
    }

    protected override async Task HandleDesiredPropertiesChangedAsync(JToken? token, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received desired properties changed call with payload: {Payload}",
            token?.ToString());
    }
}
```

### Async Timers / Batch Event Processor

```csharp
public class MessageReader : HostedTimerService
{
    private readonly BatchEventProcessor _batchEventProcessor;

    public MessageReader(IModuleClient moduleClient, ILogger<MessageReader> logger)
        : base(interval: TimeSpan.FromSeconds(60), shouldCallInitially: true, shouldWaitForElapsedToComplete: true)
    {
        // max capacity = 1000, timeout = 60 seconds 
        _batchEventProcessor = new BatchEventProcessor(moduleClient, 1000, TimeSpan.FromSeconds(60), logger);
        _batchEventProcessor.Start();
    }

    protected override async Task ElapsedAsync(CancellationToken cancellationToken)
    {
        var data = await ReadDataFromSomewhereAsync();

        var message = MessageFactory.CreateMessage(data);
        _batchEventProcessor.EnqueueEvent(message);
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
