using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;

namespace AIT.Devices
{
    public interface IMethodHandler
    {
        Task<MethodResponse> HandleMethodAsync(MethodRequest methodRequest);
    }
}