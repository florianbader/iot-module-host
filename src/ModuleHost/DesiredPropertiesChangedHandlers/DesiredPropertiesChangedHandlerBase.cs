using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Bader.Edge.ModuleHost;

/// <summary>
/// This defines a desired property changed handler class which catches exceptions and logs them.
/// </summary>
public abstract class DesiredPropertiesChangedHandlerBase : IDesiredPropertiesChangedHandler
{
    private readonly string _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="DesiredPropertiesChangedHandlerBase"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    protected DesiredPropertiesChangedHandlerBase(ILogger logger)
    {
        Logger = logger;
        _name = GetType().FullName;
    }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    protected ILogger Logger { get; }

    /// <inheritdoc />
    async Task IDesiredPropertiesChangedHandler.HandleDesiredPropertiesChangedAsync(JToken? token, CancellationToken cancellationToken)
    {
        try
        {
            Logger.LogTrace("Executing desired property changed handler {DesiredPropertyChangedHandlerName}", _name);

            await HandleDesiredPropertiesChangedAsync(token, cancellationToken).ConfigureAwait(false);

            Logger.LogTrace("Successfully handled desired property changed {DesiredPropertyChangedHandlerName}", _name);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred handling desired property changed in {Name}", _name);
        }
    }

    /// <summary>
    /// Handles desired properties changed from the cloud.
    /// </summary>
    /// <param name="token">The JSON token of the desired properties in the twin.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    protected abstract Task HandleDesiredPropertiesChangedAsync(JToken? token, CancellationToken cancellationToken);
}
