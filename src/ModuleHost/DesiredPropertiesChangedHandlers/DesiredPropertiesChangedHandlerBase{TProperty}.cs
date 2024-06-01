using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Bader.Edge.ModuleHost;

/// <summary>
/// This defines a desired property changed handler class which catches exceptions and logs them.
/// </summary>
/// <typeparam name="TProperty">The type of the desired property.</typeparam>
public abstract class DesiredPropertiesChangedHandlerBase<TProperty> : DesiredPropertiesChangedHandlerBase
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    protected DesiredPropertiesChangedHandlerBase(JsonSerializerOptions jsonSerializerOptions, ILogger logger) : base(logger) => _jsonSerializerOptions = jsonSerializerOptions;

    /// <inheritdoc />
    protected override async Task HandleDesiredPropertiesChangedAsync(JToken? token, CancellationToken cancellationToken)
    {
        if (token is null)
        {
            await HandleDesiredPropertiesChangedAsync(null, cancellationToken).ConfigureAwait(false);
            return;
        }

        var property = JsonSerializer.Deserialize<TProperty>(token.ToString(), _jsonSerializerOptions);
        await HandleDesiredPropertiesChangedAsync(property, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Handles desired properties changed from the cloud.
    /// </summary>
    /// <param name="property">The desired property in the twin.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    protected abstract Task HandleDesiredPropertiesChangedAsync(TProperty? property, CancellationToken cancellationToken);
}
