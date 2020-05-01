using System.Text.Json;
using System.Threading.Tasks;
using AIT.Devices;
using Microsoft.Azure.Devices.Client;
using Serilog;

namespace Starter.MessageHandlers
{
    // This is not needed for default message handlers but can still be specified to explicitly mark the default message handler
    [MessageHandler(isDefault: true)]
    public class DefaultMessageHandler : IMessageHandler
    {
        private readonly IModuleClient _moduleClient;

        public DefaultMessageHandler(IModuleClient moduleClient) => _moduleClient = moduleClient;

        public async Task<MessageResponse> HandleMessageAsync(Message message)
        {
            message.Properties["original-connection-module-id"] = message.ConnectionModuleId;

            await _moduleClient.SendEventAsync(message).ConfigureAwait(false);

            var json = await JsonDocument.ParseAsync(message.BodyStream).ConfigureAwait(false);
            Log.Information("Received message: {Message}", json.RootElement.ToString());

            return MessageResponse.Completed;
        }
    }
}
