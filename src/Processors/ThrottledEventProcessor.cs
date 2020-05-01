using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bader.Edge.ModuleHost;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace AIT.Devices.Processors
{
    public class ThrottledEventProcessor : IDisposable
    {
        private readonly byte[] _comma = Encoding.UTF8.GetBytes(new[] { ',' });
        private readonly ILogger<ThrottledEventProcessor> _logger;
        private readonly int _maxMessageSize;
        private readonly int _messageHeaderSize;
        private readonly IModuleClient _moduleClient;
        private readonly BlockingCollection<Message> _queue;
        private readonly CancellationToken _shutdownToken;
        private readonly byte[] _squareBracketClose = Encoding.UTF8.GetBytes(new[] { ']' });
        private readonly byte[] _squareBracketOpen = Encoding.UTF8.GetBytes(new[] { '[' });
        private readonly TimeSpan _timeout;
        private int _enqueueCount;
        private int _enqueueFailCount;
        private bool _isDisposing;

        public ThrottledEventProcessor(IModuleClient moduleClient, int capacity, TimeSpan timeout, ILogger<ThrottledEventProcessor> logger, CancellationToken shutdownToken)
        {
            _queue = new BlockingCollection<Message>(capacity);
            _moduleClient = moduleClient;
            _timeout = timeout;
            _logger = logger;
            _shutdownToken = shutdownToken;

            // System properties size: MessageId (max 128 bytes) + sequence number (ulong) + expiry date (DateTime)
            _messageHeaderSize = 128 + sizeof(ulong) + DateTime.UtcNow.ToString(CultureInfo.InvariantCulture).Length;

            // TODO: Make this a constructor parameter.
            _maxMessageSize = 1024 * 4;
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
            var nextSendTime = DateTime.UtcNow + _timeout;

            var memoryStream = new MemoryStream();
            memoryStream.Write(_squareBracketOpen, 0, 1);

            var properties = new Dictionary<string, string>();
            var applicationPropertySize = 0;
            Message? message = null;

            while (!_shutdownToken.IsCancellationRequested)
            {
                try
                {
                    var millisecondsTimeout = (int)nextSendTime.Subtract(DateTime.UtcNow).TotalMilliseconds;
                    var hasMessage = _queue.TryTake(out message, millisecondsTimeout, _shutdownToken);

                    if (!hasMessage)
                    {
                        continue;
                    }

                    var messageBytes = (message.BodyStream as MemoryStream)?.ToArray() ?? throw new InvalidOperationException("Invalid body stream in message");
                    var messagePropertiesSize = GetApplicationPropertiesLength(properties, message);

                    var aggregatedMessageSize = _messageHeaderSize + applicationPropertySize + messagePropertiesSize + messageBytes.Length + memoryStream.Length;

                    // check if we would either exceed max message size or the next send time
                    var shouldSendMessage = aggregatedMessageSize > _maxMessageSize || DateTime.UtcNow <= nextSendTime;
                    if (!shouldSendMessage)
                    {
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
                        var offset = messageBytes[0] == _squareBracketOpen[0] ? 1 : 0;
                        var length = messageBytes[0] == _squareBracketOpen[0] ? messageBytes.Length - 1 : messageBytes.Length;
                        memoryStream.Write(messageBytes, offset, length);
                        memoryStream.Write(_comma, 0, 1);

                        continue;
                    }

                    memoryStream.Write(_squareBracketClose, 0, 1);

                    // send message
                    var hubMessage = new Message(memoryStream);

                    foreach (var key in properties.Keys)
                    {
                        hubMessage.Properties[key] = properties[key];
                    }

                    try
                    {
                        await _moduleClient.SendEventAsync(hubMessage, _shutdownToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        // reset
                        hubMessage?.Dispose();

                        properties.Clear();
                        applicationPropertySize = 0;

                        memoryStream.Position = 0;
                        memoryStream.SetLength(0);
                        memoryStream.Write(_squareBracketOpen, 0, 1);

                        nextSendTime = DateTime.UtcNow + _timeout;
                    }
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
