using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace Bader.Edge.ModuleHost
{
    /// <summary>
    /// This defines a message handler class which catches exceptions and logs them.
    /// </summary>
    public abstract class MessageHandlerBase : IMessageHandler
    {
        private readonly string _name;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlerBase"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        protected MessageHandlerBase(ILogger logger)
        {
            Logger = logger;
            _name = GetType().FullName;
        }

        /// <inheritdoc />
        async Task<MessageResponse> IMessageHandler.HandleMessageAsync(Message message)
        {
            try
            {
                Logger.LogTrace("Executing message handler {MessageHandlerName}", _name);

                var response = await HandleMessageAsync(message).ConfigureAwait(false);

                Logger.LogTrace("Successfully handled message {MethodHandlerName}", _name);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred handling message in message handler {Name}", _name);
                return Error();
            }
        }

        /// <summary>
        /// Returns a message response abandoned.
        /// </summary>
        /// <returns>A method response abandoned.</returns>
        protected MessageResponse Error() => MessageResponse.Abandoned;

        /// <summary>
        /// Handles a message from the cloud or another module. Exceptions get caught and automatically logged.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The message response.</returns>
        protected abstract Task<MessageResponse> HandleMessageAsync(Message message);

        /// <summary>
        /// Returns a message response completed.
        /// </summary>
        /// <returns>A message response completed.</returns>
        protected MessageResponse Ok() => MessageResponse.Completed;
    }
}
