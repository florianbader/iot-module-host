namespace Bader.Edge.ModuleHost;

/// <summary>
/// Defines a message handler and declares optionally if it's a default message handler or for a specific input.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MessageHandlerAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandlerAttribute"/> class specifying it's the default handler.
    /// </summary>
    public MessageHandlerAttribute() : this(TimeSpan.FromSeconds(30))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandlerAttribute"/> class specifying it's the default handler.
    /// </summary>
    /// <param name="timeout">The timeout of the message handler. Default is 30 seconds.</param>
    public MessageHandlerAttribute(TimeSpan timeout) => Timeout = timeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandlerAttribute"/> class specifying if the name of the message input in the route.
    /// </summary>
    /// <param name="inputName">The input name of the handled method.</param>
    public MessageHandlerAttribute(string inputName) : this(inputName, TimeSpan.FromSeconds(30))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageHandlerAttribute"/> class specifying if the name of the message input in the route.
    /// </summary>
    /// <param name="inputName">The input name of the handled method.</param>
    /// <param name="timeout">The timeout of the message handler. Default is 30 seconds.</param>
    public MessageHandlerAttribute(string inputName, TimeSpan timeout)
    {
        InputName = inputName;
        Timeout = timeout;
    }

    /// <summary>
    /// Gets the input name of the handled message.
    /// </summary>
    public string? InputName { get; }

    /// <summary>
    /// Gets the timeout of the message handler. The default is 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; }
}
