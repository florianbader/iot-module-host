using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace Bader.Edge.ModuleHost.Tests.Processors;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "IDISP001: Dispose created", Justification = "Will be disposed in enqueue event.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "IDISP004: Don't ignore created IDisposable", Justification = "Will be disposed in enqueue event.")]
public sealed class BatchEventProcessorTests : IAsyncLifetime, IDisposable
{
    private readonly DateTime _defaultSystemTime;
    private readonly Mock<IModuleClient> _moduleClientMock;
    private readonly BatchEventProcessor _processor;
    private readonly Mock<ISystemTime> _systemTimeMock;

    public BatchEventProcessorTests()
    {
        _moduleClientMock = new Mock<IModuleClient>();

        _defaultSystemTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _systemTimeMock = new Mock<ISystemTime>();
        _systemTimeMock.SetupGet(s => s.UtcNow).Returns(_defaultSystemTime);

        _processor = new BatchEventProcessor(_moduleClientMock.Object, 100, 1024 * 4, TimeSpan.FromSeconds(2), _systemTimeMock.Object, Mock.Of<ILogger<BatchEventProcessor>>());
    }

    [Fact]
    public void AboveMaxMessageSizeShouldNotSendCurrentMesageBatch()
    {
        var message1 = new Message(new byte[1024]);
        _processor.EnqueueEvent(message1);

        var message2 = new Message(new byte[_processor.MaxMessageSize]);
        _processor.EnqueueEvent(message2);

        WaitUntil(() => _processor.SendCount >= 1);

        _moduleClientMock.Verify(m => m.SendEventAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void BelowMaxMessageSizeShouldNotSendAMessage()
    {
        var message1 = new Message(new byte[1024]);

        _processor.EnqueueEvent(message1);

        _moduleClientMock.Verify(m => m.SendEventAsync(It.IsAny<Message>()), Times.Never);
    }

    public void Dispose() => _processor.Dispose();

    public Task DisposeAsync()
    {
        _processor.Stop();
        return Task.CompletedTask;
    }

    public Task InitializeAsync()
    {
        _processor.Start();
        return Task.CompletedTask;
    }

    [Fact]
    public void MessagePropertiesShouldBeConcatenated()
    {
        Message? actualMessage = null;
        _moduleClientMock.Setup(m => m.SendEventAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>())).Callback<Message, CancellationToken>((m, _) => actualMessage = m);

        _processor.EnqueueEvent(new Message(Array.Empty<byte>())
        {
            Properties =
            {
                { "prop1", "value1" },
                { "prop2", "value2" },
            },
        });

        WaitUntil(() => _processor.ProcessedCount == 1);

        _processor.EnqueueEvent(new Message(Array.Empty<byte>())
        {
            Properties =
            {
                { "prop1", "value0" },
                { "prop3", "value3" },
            },
        });

        WaitUntil(() => _processor.ProcessedCount == 2);

        _systemTimeMock.SetupGet(s => s.UtcNow).Returns(_defaultSystemTime + _processor.Timeout);

        // we enqueue a third message to trigger the first two
        _processor.EnqueueEvent(CreateMessage(string.Empty));

        WaitUntil(() => _processor.SendCount >= 1);

        var actualProperties = actualMessage?.Properties;

        actualProperties.Should().Contain(new KeyValuePair<string, string>("prop1", "value1"));
        actualProperties.Should().Contain(new KeyValuePair<string, string>("prop2", "value2"));
        actualProperties.Should().Contain(new KeyValuePair<string, string>("prop3", "value3"));
    }

    [Fact]
    public void MessageShouldBeSendAfterTimeoutIsReached()
    {
        var message1 = new Message(new byte[1]);
        _processor.EnqueueEvent(message1);

        WaitUntil(() => _processor.ProcessedCount == 1);

        _systemTimeMock.SetupGet(s => s.UtcNow).Returns(_defaultSystemTime + _processor.Timeout);

        var message2 = new Message(new byte[1]);
        _processor.EnqueueEvent(message2);

        WaitUntil(() => _processor.SendCount >= 1);

        _moduleClientMock.Verify(m => m.SendEventAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void MessagesShouldBeConcatenatedInArray()
    {
        const string payload1 = @"{ ""test"": 123, ""test2"": 456 }";
        const string payload2 = @"{ ""test3"": 321, ""test4"": 654 }";
        const string expectedPayload = "[" + payload1 + "," + payload2 + "]";

        string? actualPayload = null;
        _moduleClientMock.Setup(m => m.SendEventAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>())).Callback<Message, CancellationToken>((m, _) => actualPayload = GetPayload(m));

        _processor.EnqueueEvent(CreateMessage(payload1));

        WaitUntil(() => _processor.ProcessedCount == 1);

        _processor.EnqueueEvent(CreateMessage(payload2));

        WaitUntil(() => _processor.ProcessedCount == 2);

        _systemTimeMock.SetupGet(s => s.UtcNow).Returns(_defaultSystemTime + _processor.Timeout);

        // we enqueue a third message to trigger the first two
        _processor.EnqueueEvent(CreateMessage(string.Empty));

        WaitUntil(() => _processor.SendCount >= 1);

        actualPayload.Should().Be(expectedPayload);
    }

    [Fact]
    public void MessagesWithArrayShouldBeConcatenatedInArray()
    {
        const string payload1 = @"{ ""test"": 123, ""test2"": 456 }";
        const string payload2 = @"{ ""test3"": 321, ""test4"": 654 }";
        const string expectedPayload = "[" + payload1 + "," + payload2 + "]";

        string? actualPayload = null;
        _moduleClientMock.Setup(m => m.SendEventAsync(It.IsAny<Message>(), It.IsAny<CancellationToken>())).Callback<Message, CancellationToken>((m, _) => actualPayload = GetPayload(m));

        _processor.EnqueueEvent(CreateMessage("[" + payload1 + "]"));

        WaitUntil(() => _processor.ProcessedCount == 1);

        _processor.EnqueueEvent(CreateMessage("[" + payload2 + "]"));

        WaitUntil(() => _processor.ProcessedCount == 2);

        _systemTimeMock.SetupGet(s => s.UtcNow).Returns(_defaultSystemTime + _processor.Timeout);

        // we enqueue a third message to trigger the first two
        _processor.EnqueueEvent(CreateMessage(string.Empty));

        WaitUntil(() => _processor.SendCount >= 1);

        actualPayload.Should().Be(expectedPayload);
    }

    private Message CreateMessage(string payload) => new Message(Encoding.UTF8.GetBytes(payload));

    private string GetPayload(Message message) => Encoding.UTF8.GetString(((MemoryStream)message.BodyStream).ToArray());

    private void WaitUntil(Func<bool> predicate) => WaitUntil(predicate, TimeSpan.FromSeconds(5));

    private void WaitUntil(Func<bool> predicate, TimeSpan timeout)
    {
        var timeoutTime = DateTime.UtcNow + timeout;

        while (!predicate() && DateTime.UtcNow <= timeoutTime)
        {
            Thread.Sleep(100);
        }
    }
}
