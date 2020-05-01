using System.Text.Json;
using System.Threading.Tasks;
using AIT.Devices;
using Microsoft.Azure.Devices.Client;
using Serilog;

namespace Starter.MessageHandlers
{
    [MessageHandler("input1")]
    public class Input1MessageHandler : IMessageHandler
    {
        private readonly IModuleClient _moduleClient;

        public Input1MessageHandler(IModuleClient moduleClient) => _moduleClient = moduleClient;

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
