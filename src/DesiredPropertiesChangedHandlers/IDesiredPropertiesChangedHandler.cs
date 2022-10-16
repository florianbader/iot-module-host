using Newtonsoft.Json.Linq;

namespace Bader.Edge.ModuleHost;

/// <summary>
/// This defines a desired property changed handler class.
/// </summary>
public interface IDesiredPropertiesChangedHandler
{
    /// <summary>
    /// Handles desired properties changed from the cloud.
    /// </summary>
    /// <param name="token">The JSON token of the desired propertis in the twin.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The task.</returns>
    Task HandleDesiredPropertiesChangedAsync(JToken? token, CancellationToken cancellationToken);
}
