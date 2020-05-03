using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Bader.Edge.ModuleHost
{
    public abstract class HostedTimerService : IHostedService, IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource;

        protected TimeSpan Interval { get; }

        protected bool ShouldCallInitially { get; }

        protected bool ShouldWaitForElapsedToComplete { get; }

        protected HostedTimerService(TimeSpan interval, bool shouldCallInitially = false, bool shouldWaitForElapsedToComplete = true)
        {
            Interval = interval;
            ShouldCallInitially = shouldCallInitially;
            ShouldWaitForElapsedToComplete = shouldWaitForElapsedToComplete;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose() => _cancellationTokenSource.Dispose();

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (ShouldCallInitially)
            {
                await ElapsedAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
            }

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(Interval, _cancellationTokenSource.Token).ConfigureAwait(false);

                if (ShouldWaitForElapsedToComplete)
                {
                    await ElapsedAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
                }
                else
                {
                    _ = Task.Run(() => ElapsedAsync(_cancellationTokenSource.Token));
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            return Task.CompletedTask;
        }

        protected abstract Task ElapsedAsync(CancellationToken cancellationToken);
    }
}
