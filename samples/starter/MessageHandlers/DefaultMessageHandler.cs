using System.Text.Json;
using Bader.Edge.ModuleHost;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace Starter.MessageHandlers;

[MessageHandler]
public class DefaultMessageHandler : MessageHandlerBase
{
    private readonly ILogger<DefaultMessageHandler> _logger;
    private readonly IModuleClient _moduleClient;

    public DefaultMessageHandler(IModuleClient moduleClient, ILogger<DefaultMessageHandler> logger) : base(logger)
    {
        _moduleClient = moduleClient;
        _logger = logger;
    }

    protected override async Task<MessageResponse> HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        message.Properties["original-connection-module-id"] = message.ConnectionModuleId;

        await _moduleClient.SendEventAsync(message).ConfigureAwait(false);

        using var json = await JsonDocument.ParseAsync(message.BodyStream).ConfigureAwait(false);
        _logger.LogInformation("Received message: {Message}", json.RootElement.ToString());

        return Ok();
    }
}
