using System.Text;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace Bader.Edge.ModuleHost;

/// <summary>
/// Creates messages from objects.
/// </summary>
public static class MessageFactory
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings();

    /// <summary>
    /// Create a new message by serializing the given object into UTF-8 bytes.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="jsonSerializerSettings">The json serializer settings.</param>
    /// <returns>A new message.</returns>
    public static Message CreateMessage<T>(T obj, JsonSerializerSettings jsonSerializerSettings) => new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj, jsonSerializerSettings)));

    /// <summary>
    /// Create a new message by serializing the given object into UTF-8 bytes.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A new message.</returns>
    public static Message CreateMessage<T>(T obj) => CreateMessage(obj, JsonSerializerSettings);
}
