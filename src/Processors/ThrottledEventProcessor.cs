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
        private readonly ILogger<ThrottledEventProcessor> _logger;
        private readonly IModuleClient _moduleClient;
        private readonly BlockingCollection<Message> _queue;
        private readonly CancellationToken _shutdownToken;
        private readonly ISystemTime _systemTime;
        private int _enqueueCount;
        private int _enqueueFailCount;
        private bool _isDisposing;
        private int _processedCount;
        private int _sendCount;

        public int EnqueueCount => _enqueueCount;

        public int EnqueueFailCount => _enqueueFailCount;

        public int MaxMessageSize { get; }

        public int ProcessedCount => _processedCount;

        public int SendCount => _sendCount;

        public TimeSpan Timeout { get; }

        internal int MessageHeaderSize { get; }

        public ThrottledEventProcessor(IModuleClient moduleClient, int capacity, TimeSpan timeout, ISystemTime systemTime, ILogger<ThrottledEventProcessor> logger, CancellationToken shutdownToken)
            : this(moduleClient, capacity, 1024 * 4, timeout, systemTime, logger, shutdownToken)
        {
        }

        public ThrottledEventProcessor(IModuleClient moduleClient, int capacity, int maxMessageSize, TimeSpan timeout, ISystemTime systemTime, ILogger<ThrottledEventProcessor> logger, CancellationToken shutdownToken)
        {
            _queue = new BlockingCollection<Message>(capacity);
            _moduleClient = moduleClient;
            Timeout = timeout;
            _systemTime = systemTime;
            _logger = logger;
            _shutdownToken = shutdownToken;

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

        public bool EnqueueEvent(Message message)
        {
            Interlocked.Increment(ref _enqueueCount);
            if (!_queue.TryAdd(message))
            {
                Interlocked.Increment(ref _enqueueFailCount);
                _logger.LogError("Could not enqueue event. We have already lost {FailedMessageCount} events.", _enqueueFailCount);
                return false;
            }

            return true;
        }

        public async Task StartAsync()
        {
            var nextSendTime = _systemTime.UtcNow + Timeout;

            var memoryStream = new MemoryStream();
            memoryStream.Write(SquareBracketOpen, 0, 1);

            var properties = new Dictionary<string, string>();
            var applicationPropertySize = 0;
            Message? message = null;

            while (!_shutdownToken.IsCancellationRequested)
            {
                try
                {
                    var millisecondsTimeout = (int)nextSendTime.Subtract(_systemTime.UtcNow).TotalMilliseconds;
                    var hasMessage = _queue.TryTake(out message, millisecondsTimeout, _shutdownToken);

                    if (!hasMessage)
                    {
                        continue;
                    }

                    var messageBytes = (message.BodyStream as MemoryStream)?.ToArray() ?? throw new InvalidOperationException("Invalid body stream in message");
                    var messagePropertiesSize = GetApplicationPropertiesLength(properties, message);

                    var aggregatedMessageSize = MessageHeaderSize + applicationPropertySize + messagePropertiesSize + messageBytes.Length + memoryStream.Length;

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
                            await _moduleClient.SendEventAsync(hubMessage, _shutdownToken).ConfigureAwait(false);
                            Interlocked.Increment(ref _sendCount);
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

                    if (memoryStream.Length > 1)
                    {
                        memoryStream.Write(Comma, 0, 1);
                    }

                    // append all properties of the current message if they do not already exist
                    // TODO: Make this behaviour public so the user can overwrite it.
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposing)
            {
                if (disposing)
                {
                    _queue.Dispose();
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
    }
}
