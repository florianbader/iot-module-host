using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace Bader.Edge.ModuleHost
{
    /// <summary>
    /// This defines a message handler class.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Handles a message from the cloud or another module.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The message response.</returns>
        Task<MessageResponse> HandleMessageAsync(Message message);
    }
}
