using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace Bader.Edge.ModuleHost
{
    public class ThrottledEventProcessor : IDisposable
    {
        private static readonly byte[] Comma = Encoding.UTF8.GetBytes(new[] { ',' });
        private static readonly byte[] SquareBracketClose = Encoding.UTF8.GetBytes(new[] { ']' });
        private static readonly byte[] SquareBracketOpen = Encoding.UTF8.GetBytes(new[] { '[' });
        private readonly ILogger _logger;
        private readonly IModuleClient _moduleClient;
        private readonly BlockingCollection<Message> _queue;
        private readonly ISystemTime _systemTime;
        private CancellationTokenSource _cancellationTokenSource;
        private int _enqueueCount;
        private int _enqueueFailCount;
        private bool _isDisposing;
        private int _processedCount;
        private int _sendCount;

        /// <summary>
        /// Gets the enqueued messages count.
        /// </summary>
        public int EnqueueCount => _enqueueCount;

        /// <summary>
        /// Gets the enqueued messages fail count.
        /// </summary>
        public int EnqueueFailCount => _enqueueFailCount;

        /// <summary>
        /// Gets the max message size after which the throttled messages are sent at the latest.
        /// </summary>
        public int MaxMessageSize { get; }

        /// <summary>
        /// Gets the processed events count.
        /// </summary>
        public int ProcessedCount => _processedCount;

        /// <summary>
        /// Gets the send messages count.
        /// </summary>
        public int SendCount => _sendCount;

        /// <summary>
        /// Gets the timeout after which the throttled messages are sent at the latest.
        /// </summary>
        public TimeSpan Timeout { get; }

        internal int MessageHeaderSize { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottledEventProcessor"/> class.
        /// </summary>
        /// <param name="moduleClient">The module client.</param>
        /// <param name="capacity">The maximum capacity of the message queue. If the capacity is reached new messages won't get enqueued.</param>
        /// <param name="timeout">The timeout after which the throttled messages are sent at the latest.</param>
        /// <param name="logger">The logger.</param>
        public ThrottledEventProcessor(IModuleClient moduleClient, int capacity, TimeSpan timeout, ILogger logger)
            : this(moduleClient, capacity, 1024 * 4, timeout, new SystemTime(), logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottledEventProcessor"/> class.
        /// </summary>
        /// <param name="moduleClient">The module client.</param>
        /// <param name="capacity">The maximum capacity of the message queue. If the capacity is reached new messages won't get enqueued.</param>
        /// <param name="maxMessageSize">The maximum message size after which the throttled messages are sent at the latest. </param>
        /// <param name="timeout">The timeout after which the throttled messages are sent at the latest.</param>
        /// <param name="logger">The logger.</param>
        public ThrottledEventProcessor(IModuleClient moduleClient, int capacity, int maxMessageSize, TimeSpan timeout, ILogger logger)
            : this(moduleClient, capacity, maxMessageSize, timeout, new SystemTime(), logger)
        {
        }

        internal ThrottledEventProcessor(IModuleClient moduleClient, int capacity, int maxMessageSize, TimeSpan timeout, ISystemTime systemTime, ILogger logger)
        {
            _queue = new BlockingCollection<Message>(capacity);
            _moduleClient = moduleClient;
            Timeout = timeout;
            _systemTime = systemTime;
            _logger = logger;

            _cancellationTokenSource = new CancellationTokenSource();

            // System properties size: MessageId (max 128 bytes) + sequence number (ulong) + expiry date (DateTime)
            MessageHeaderSize = 128 + sizeof(ulong) + _systemTime.UtcNow.ToString(CultureInfo.InvariantCulture).Length;

            MaxMessageSize = maxMessageSize;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Enqueue a new message. The message will automatically be disposed after it is sent. If the enqueue fails the message won't get disposed.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Whether the enqueue of the messages was successfull.</returns>
        public bool EnqueueEvent(Message message)
        {
            _ = message ?? throw new ArgumentNullException(nameof(message));

            Interlocked.Increment(ref _enqueueCount);

            if (!_queue.TryAdd(message))
            {
                Interlocked.Increment(ref _enqueueFailCount);
                _logger.LogError("Could not enqueue event. We have already lost {FailedMessageCount} events.", _enqueueFailCount);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Start the processor in a new background thread.
        /// </summary>
        public void Start()
            => new Thread(() => StartInternalAsync().Wait())
            {
                IsBackground = true,
            }.Start();

        /// <summary>
        /// Stops the processor.
        /// </summary>
        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposing)
            {
                if (disposing)
                {
                    _queue.Dispose();
                    _cancellationTokenSource.Dispose();
                }

                _isDisposing = true;
            }
        }

        private int GetApplicationPropertiesLength(IDictionary<string, string> properties, Message message)
        {
            var applicationPropertySize = 0;

            foreach (var property in message.Properties.Keys)
            {
                if (!properties.ContainsKey(property) && message.Properties[property] != null)
                {
                    properties.Add(property, message.Properties[property]);

                    applicationPropertySize += property.Length;
                    applicationPropertySize += message.Properties[property].Length;
                }
            }

            return applicationPropertySize;
        }

        private async Task StartInternalAsync()
        {
            var nextSendTime = _systemTime.UtcNow + Timeout;

            var memoryStream = new MemoryStream();
            memoryStream.Write(SquareBracketOpen, 0, 1);

            var properties = new Dictionary<string, string>();
            var applicationPropertySize = 0;
            Message? message = null;

            _logger.LogDebug("Starting throttled event processor");

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var millisecondsTimeout = (int)nextSendTime.Subtract(_systemTime.UtcNow).TotalMilliseconds;
                    var hasMessage = _queue.TryTake(out message, millisecondsTimeout, _cancellationTokenSource.Token);

                    if (!hasMessage)
                    {
                        continue;
                    }

                    var messageBytes = (message.BodyStream as MemoryStream)?.ToArray() ?? throw new InvalidOperationException("Invalid body stream in message");
                    var messagePropertiesSize = GetApplicationPropertiesLength(properties, message);

                    var aggregatedMessageSize = MessageHeaderSize + applicationPropertySize + messagePropertiesSize + messageBytes.Length + memoryStream.Length;
                    _logger.LogTrace("Current aggregation size: {AggregationSize}", aggregatedMessageSize);

                    // check if we would either exceed max message size or the next send time
                    var shouldSendMessage = aggregatedMessageSize > MaxMessageSize || _systemTime.UtcNow >= nextSendTime;
                    if (shouldSendMessage)
                    {
                        memoryStream.Write(SquareBracketClose, 0, 1);

                        // send message
                        var hubMessage = new Message(memoryStream);

                        foreach (var key in properties.Keys)
                        {
                            hubMessage.Properties[key] = properties[key];
                        }

                        try
                        {
                            _logger.LogTrace("Throttling criteria met, sending aggregated message...");

                            await _moduleClient.SendEventAsync(hubMessage, _cancellationTokenSource.Token).ConfigureAwait(false);
                            Interlocked.Increment(ref _sendCount);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send aggregated message");
                        }
                        finally
                        {
                            // reset
                            hubMessage?.Dispose();

                            properties.Clear();
                            applicationPropertySize = 0;

                            memoryStream.Position = 0;
                            memoryStream.SetLength(0);
                            memoryStream.Write(SquareBracketOpen, 0, 1);

                            nextSendTime = _systemTime.UtcNow + Timeout;
                        }
                    }

                    _logger.LogTrace("Throttling message and waiting for next message.");

                    if (memoryStream.Length > 1)
                    {
                        memoryStream.Write(Comma, 0, 1);
                    }

                    // append all properties of the current message if they do not already exist
                    // TODO: Make this behavior public so the user can overwrite it.
                    foreach (var property in message.Properties.Keys)
                    {
                        if (!properties.ContainsKey(property) && message.Properties[property] != null)
                        {
                            properties.Add(property, message.Properties[property]);
                        }
                    }

                    applicationPropertySize += messagePropertiesSize;

                    // if we received an array we assume it is already batched and we just merge the content
                    var offset = messageBytes[0] == SquareBracketOpen[0] ? 1 : 0;
                    var length = messageBytes[0] == SquareBracketOpen[0] ? messageBytes.Length - 2 : messageBytes.Length;
                    memoryStream.Write(messageBytes, offset, length);

                    Interlocked.Increment(ref _processedCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing messages.");
                }
                finally
                {
                    message?.Dispose();
                }
            }

            memoryStream?.Dispose();
        }
    }
}
