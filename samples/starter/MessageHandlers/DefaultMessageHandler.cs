using System.Text.Json;
using System.Threading.Tasks;
using Bader.Edge.ModuleHost;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace Starter.MessageHandlers
{
    // This is not needed for default message handlers but can still be specified to explicitly mark the default message handler
    [MessageHandler(isDefault: true)]
    public class DefaultMessageHandler : MessageHandlerBase
    {
        private readonly IModuleClient _moduleClient;
        private readonly ILogger<DefaultMessageHandler> _logger;

        public DefaultMessageHandler(IModuleClient moduleClient, ILogger<DefaultMessageHandler> logger) : base(logger)
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
