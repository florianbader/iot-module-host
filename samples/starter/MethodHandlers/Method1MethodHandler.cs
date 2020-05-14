using System.Text.Json;
using System.Threading.Tasks;
using Bader.Edge.ModuleHost;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace Starter.MessageHandlers
{
    [MethodHandler("method1")]
    public class Method1MethodHandler : MethodHandlerBase
    {
        private readonly IModuleClient _moduleClient;
        private readonly ILogger<Method1MethodHandler> _logger;

        public Method1MethodHandler(IModuleClient moduleClient, ILogger<Method1MethodHandler> logger) : base(logger)
        {
            _moduleClient = moduleClient;
            _logger = logger;
        }

        protected override async Task<MethodResponse> HandleMethodAsync(MethodRequest methodRequest)
        {
            var json = JsonDocument.Parse(methodRequest.DataAsJson);
            _logger.LogInformation("Received method call '{MethodName}' with payload: {Payload}",
                methodRequest.Name, json.RootElement.ToString());

            return Ok();
        }
    }
}
