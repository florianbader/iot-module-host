using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Bader.Edge.ModuleHost;

/// <summary>
/// Defines the start up of an edge module.
/// </summary>
public interface IStartup
{
    /// <summary>
    /// Configures the services in the provided service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    void ConfigureServices(IServiceCollection services);

    /// <summary>
    /// Gets called when the connection status changes.
    /// </summary>
    /// <param name="serviceProvider">The services.</param>
    /// <param name="status">The new connection status.</param>
    /// <param name="reason">The reason for a connection status change.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ConnectionStatusChangesAsync(IServiceProvider serviceProvider, ConnectionStatus status, ConnectionStatusChangeReason reason);

    /// <summary>
    /// Gets called when a desired property in the module twin updates.
    /// </summary>
    /// <param name="serviceProvider">The services.</param>
    /// <param name="desiredProperties">The desired properties of the module twin.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DesiredPropertyUpdateAsync(IServiceProvider serviceProvider, TwinCollection desiredProperties);

    /// <summary>
    /// Ability to configure services asynchronously. All services are already configured and the module client is connected at this point.
    /// </summary>
    /// <param name="serviceProvider">The services.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task InitializeAsync(IServiceProvider serviceProvider);
}
