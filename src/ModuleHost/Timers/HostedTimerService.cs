using Microsoft.Extensions.Hosting;

namespace Bader.Edge.ModuleHost;

/// <summary>
/// A hosted timer service which calls an asynchronous action after the configured interval.
/// </summary>
public abstract class HostedTimerService : IHostedService, IDisposable
{
    private CancellationTokenSource _cancellationTokenSource;

    private bool _isDisposing = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="HostedTimerService"/> class.
    /// </summary>
    /// <param name="interval">The interval of the timer.</param>
    /// <param name="shouldCallInitially">Whether the action should be called once when starting the timer.</param>
    /// <param name="shouldWaitForElapsedToComplete">Whether the timer should wait for the action to complete before starting the next interval.</param>
    protected HostedTimerService(TimeSpan interval, bool shouldCallInitially = false, bool shouldWaitForElapsedToComplete = true)
    {
        Interval = interval;
        ShouldCallInitially = shouldCallInitially;
        ShouldWaitForElapsedToComplete = shouldWaitForElapsedToComplete;
        _cancellationTokenSource = new CancellationTokenSource();
    }

    ~HostedTimerService()
    {
        Dispose(false);
    }

    /// <summary>
    /// Gets the cancellation token of the timer service which signals a timer stop.
    /// </summary>
    protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

    /// <summary>
    /// Gets or sets the interval of the timer. This can be changed even after the timer was started and will be used on the next round.
    /// </summary>
    protected TimeSpan Interval { get; set; }

    /// <summary>
    /// Gets a value indicating whether the action should be called once when starting the timer.
    /// </summary>
    protected bool ShouldCallInitially { get; }

    /// <summary>
    /// Gets a value indicating whether the timer should wait for the action to complete before starting the next interval.
    /// </summary>
    protected bool ShouldWaitForElapsedToComplete { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        if (ShouldCallInitially)
        {
            await ElapsedAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
        }

        new Thread(async () =>
        {
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
        })
        {
            IsBackground = true,
        }.Start();
    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposing)
        {
            if (disposing)
            {
                _cancellationTokenSource.Dispose();
            }

            _isDisposing = true;
        }
    }

    /// <summary>
    /// The action which should be called each time the timer elapses.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token of the timer service which signals a timer stop.</param>
    protected abstract Task ElapsedAsync(CancellationToken cancellationToken);
}
