using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bader.Edge.ModuleHost;
using Microsoft.Extensions.Logging;

namespace Starter.Timers
{
    public class MessageReader : HostedTimerService
    {
        private readonly ThrottledEventProcessor _throttledEventProcessor;

        public MessageReader(IModuleClient moduleClient, ILogger<MessageReader> logger) : base(interval: TimeSpan.FromSeconds(60), shouldCallInitially: true, shouldWaitForElapsedToComplete: true)
            => _throttledEventProcessor = new ThrottledEventProcessor(moduleClient, 1000, TimeSpan.FromSeconds(60), logger, CancellationToken);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed in event processor")]
        protected override async Task ElapsedAsync(CancellationToken cancellationToken)
        {
            var data = await ReadDataFromSomewhereAsync();

            var message = MessageFactory.CreateMessage(data);
            _throttledEventProcessor.EnqueueEvent(message);
        }

        private Task<IEnumerable<object>> ReadDataFromSomewhereAsync() => Task.FromResult(Enumerable.Empty<object>());
    }
}
