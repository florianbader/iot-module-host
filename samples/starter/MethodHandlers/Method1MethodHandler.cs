using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using AIT.Devices;
using Microsoft.Azure.Devices.Client;
using Serilog;

namespace Starter.MessageHandlers
{
    [MethodHandler("method1")]
    public class Method1MethodHandler : IMethodHandler
    {
        private readonly IModuleClient _moduleClient;

        public Method1MethodHandler(IModuleClient moduleClient) => _moduleClient = moduleClient;

        public Task<MethodResponse> HandleMethodAsync(MethodRequest methodRequest)
        {
            var json = JsonDocument.Parse(methodRequest.DataAsJson);
            Log.Information("Received method call '{MethodName}' with payload: {Payload}",
                methodRequest.Name, json.RootElement.ToString());

            return Task.FromResult(new MethodResponse((int)HttpStatusCode.OK));
        }
    }
}
