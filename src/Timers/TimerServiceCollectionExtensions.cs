using Bader.Edge.ModuleHost;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TimerServiceCollectionExtensions
    {
        public static IServiceCollection AddHostedTimerService<T>(this IServiceCollection serviceCollection) where T : HostedTimerService
            => serviceCollection.AddHostedService<T>();
    }
}
