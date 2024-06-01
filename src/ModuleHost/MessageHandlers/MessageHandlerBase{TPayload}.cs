using System.Text;
using System.Text.Json;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace Bader.Edge.ModuleHost;

public abstract class MessageHandlerBase<TPayload> : MessageHandlerBase
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    protected MessageHandlerBase(JsonSerializerOptions jsonSerializerOptions, ILogger logger) : base(logger) => _jsonSerializerOptions = jsonSerializerOptions;

    protected override Task<MessageResponse> HandleMessageAsync(Message message, CancellationToken cancellationToken)
    {
        var bytes = message.GetBytes();
        var json = Encoding.UTF8.GetString(bytes);
        var payload = JsonSerializer.Deserialize<TPayload>(json, _jsonSerializerOptions);

        return HandleMessageAsync(payload, cancellationToken);
    }

    /// <summary>
    /// Handles a message from the cloud or another module. Exceptions get caught and automatically logged.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The message response.</returns>
    protected abstract Task<MessageResponse> HandleMessageAsync(TPayload? payload, CancellationToken cancellationToken);
}
