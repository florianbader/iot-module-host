using System.Text.Json;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace Bader.Edge.ModuleHost;

public abstract class MethodHandlerBase<TPayload> : MethodHandlerBase
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    protected MethodHandlerBase(JsonSerializerOptions jsonSerializerOptions, ILogger logger) : base(logger) => _jsonSerializerOptions = jsonSerializerOptions;

    /// <summary>
    /// Handles a method from the cloud or another module.
    /// </summary>
    /// <param name="payload">The payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The message response.</returns>
    protected abstract Task<MethodResponse> HandleMethodAsync(TPayload? payload, CancellationToken cancellationToken);

    protected override Task<MethodResponse> HandleMethodAsync(MethodRequest methodRequest, CancellationToken cancellationToken)
    {
        var json = methodRequest.DataAsJson;
        var payload = JsonSerializer.Deserialize<TPayload>(json, _jsonSerializerOptions);

        return HandleMethodAsync(payload, cancellationToken);
    }
}
