using Bader.Edge.ModuleHost;
using Microsoft.Extensions.Logging;

namespace Starter.Timers;

public class MessageReader : HostedTimerService
{
    private readonly ILogger<MessageReader> _logger;
    private readonly Random _random = new Random();
    private readonly BatchEventProcessor _throttledEventProcessor;

    public MessageReader(IModuleClient moduleClient, ILogger<MessageReader> logger) : base(interval: TimeSpan.FromSeconds(1), shouldCallInitially: true, shouldWaitForElapsedToComplete: true)
    {
        _throttledEventProcessor = new BatchEventProcessor(moduleClient, 1000, TimeSpan.FromSeconds(20), logger);
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _throttledEventProcessor.Start();

        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _throttledEventProcessor.Stop();

        await base.StopAsync(cancellationToken);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed in event processor")]
    protected override async Task ElapsedAsync(CancellationToken cancellationToken)
    {
        var data = await ReadDataFromSomewhereAsync();

        var message = MessageFactory.CreateMessage(data);

        _logger.LogTrace("Enqueueing sensor data message...");
        _throttledEventProcessor.EnqueueEvent(message);
    }

    private Task<SensorData> ReadDataFromSomewhereAsync() => Task.FromResult(new SensorData(_random.Next(-15, 40), _random.Next(0, 100)));

    public class SensorData
    {
        public SensorData(decimal temperature, decimal humidity)
        {
            Temperature = temperature;
            Humidity = humidity;
        }

        public decimal Humidity { get; }

        public decimal Temperature { get; }

        public DateTime TimestampUtc { get; } = DateTime.UtcNow;
    }
}
