using System.Text.Json;
using System.Threading.Tasks;
using Bader.Edge.ModuleHost;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace Starter.MessageHandlers
{
    [MessageHandler("input1")]
    public class Input1MessageHandler : MessageHandlerBase
    {
        private readonly IModuleClient _moduleClient;
        private readonly ILogger<Input1MessageHandler> _logger;

        public Input1MessageHandler(IModuleClient moduleClient, ILogger<Input1MessageHandler> logger) : base(logger)
        {
            _moduleClient = moduleClient;
            _logger = logger;
        }

        protected override async Task<MessageResponse> HandleMessageAsync(Message message)
        {
            message.Properties["original-connection-module-id"] = message.ConnectionModuleId;

            await _moduleClient.SendEventAsync(message).ConfigureAwait(false);

            var json = await JsonDocument.ParseAsync(message.BodyStream).ConfigureAwait(false);
            _logger.LogInformation("Received message: {Message}", json.RootElement.ToString());

            return Ok();
        }
    }
}
