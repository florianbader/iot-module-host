using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;

namespace AIT.Devices
{
    public interface IMessageHandler
    {
        Task<MessageResponse> HandleMessageAsync(Message message);
    }
}