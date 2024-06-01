namespace Bader.Edge.ModuleHost;

/// <summary>
/// Defines a method handler and declares optionally if it's a default method handler.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MethodHandlerAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MethodHandlerAttribute"/> class.
    /// </summary>
    public MethodHandlerAttribute()
        : this(true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodHandlerAttribute"/> class specifying if it's the default handler.
    /// </summary>
    /// <param name="isDefault">Whether this handler handles all methods which are not otherwise handled.</param>
    public MethodHandlerAttribute(bool isDefault) : this(isDefault, TimeSpan.FromSeconds(30))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodHandlerAttribute"/> class specifying if it's the default handler.
    /// </summary>
    /// <param name="isDefault">Whether this handler handles all methods which are not otherwise handled.</param>
    /// <param name="timeout">The timeout of the method handler. Default is 30 seconds.</param>
    public MethodHandlerAttribute(bool isDefault, TimeSpan timeout)
    {
        IsDefault = isDefault;
        MethodName = string.Empty;
        Timeout = timeout;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodHandlerAttribute"/> class.
    /// </summary>
    /// <param name="methodName">THe name of the method which should be handled.</param>
    public MethodHandlerAttribute(string methodName) : this(methodName, TimeSpan.FromSeconds(30))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodHandlerAttribute"/> class.
    /// </summary>
    /// <param name="methodName">THe name of the method which should be handled.</param>
    /// <param name="timeout">The timeout of the method handler. Default is 30 seconds.</param>
    public MethodHandlerAttribute(string methodName, TimeSpan timeout)
    {
        MethodName = methodName;
        Timeout = timeout;
    }

    /// <summary>
    /// Gets a value indicating whether this handler handles all methods which are not otherwise handled.
    /// </summary>
    public bool IsDefault { get; }

    /// <summary>
    /// Gets the method name of the handled method.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Gets the timeout of the method handler. Default is 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; }
}
