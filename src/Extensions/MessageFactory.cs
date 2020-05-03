using System.Text;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace Bader.Edge.ModuleHost
{
    public static class MessageFactory
    {
        public static Message CreateMessage<T>(T obj) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj)));
    }
}
