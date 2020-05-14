using Bader.Edge.ModuleHost;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// The service collections extensions to add a hosted timer service.
    /// </summary>
    public static class TimerServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a new hosted timer service.
        /// </summary>
        /// <typeparam name="T">The type of the timer service; needs to be a class which inherits from HostedTimerService.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddHostedTimerService<T>(this IServiceCollection serviceCollection)
            where T : HostedTimerService
            => serviceCollection.AddHostedService<T>();
    }
}
