using System.Text.Json;
using Bader.Edge.ModuleHost;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace Starter.MessageHandlers;

// This is not needed for default method handlers but can still be specified to explicitly mark the default method handler
[MethodHandler(isDefault: true)]
public class DefaultMethodHandler : MethodHandlerBase
{
    private readonly ILogger<DefaultMethodHandler> _logger;
    private readonly IModuleClient _moduleClient;

    public DefaultMethodHandler(IModuleClient moduleClient, ILogger<DefaultMethodHandler> logger) : base(logger)
    {
        _moduleClient = moduleClient;
        _logger = logger;
    }

    protected override async Task<MethodResponse> HandleMethodAsync(MethodRequest methodRequest, CancellationToken cancellationToken)
    {
        var json = JsonDocument.Parse(methodRequest.DataAsJson);
        _logger.LogInformation("Received method call '{MethodName}' with payload: {Payload}",
            methodRequest.Name, json.RootElement.ToString());

        await Task.Delay(1000); // Simulate some work

        return Ok();
    }
}
