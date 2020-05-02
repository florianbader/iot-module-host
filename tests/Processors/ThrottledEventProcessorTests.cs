using System;
using System.Threading;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bader.Edge.ModuleHost.Tests.Processors
{
    public class ThrottledEventProcessorTests : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Mock<IModuleClient> _moduleClientMock;
        private readonly ThrottledEventProcessor _processor;
        private readonly Mock<ISystemTime> _systemTimeMock;

        public ThrottledEventProcessorTests()
        {
            _moduleClientMock = new Mock<IModuleClient>();
            _systemTimeMock = new Mock<ISystemTime>();

            _cancellationTokenSource = new CancellationTokenSource();

            _processor = new ThrottledEventProcessor(_moduleClientMock.Object, 100, TimeSpan.FromSeconds(60), _systemTimeMock.Object, Mock.Of<ILogger<ThrottledEventProcessor>>(), _cancellationTokenSource.Token);

            new Thread(() => _processor.StartAsync().Wait())
            {
                IsBackground = true,
            }.Start();
        }

        [Fact]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Will be disposed in enqueue event.")]
        public void AboveMaxMessageSizeShouldNotSendCurrentMesageBatch()
        {
            var message1 = new Message(new byte[1024]);
            _processor.EnqueueEvent(message1);

            var message2 = new Message(new byte[_processor.MaxMessageSize]);
            _processor.EnqueueEvent(message2);

            Thread.Sleep(2_000);

            _moduleClientMock.Verify(m => m.SendEventAsync(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public void BelowMaxMessageSizeShouldNotSendAMesage()
        {
            var message1 = new Message(new byte[1024]);

            _processor.EnqueueEvent(message1);

            message1.Dispose();

            _moduleClientMock.Verify(m => m.SendEventAsync(It.IsAny<Message>()), Times.Never);
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _processor.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
