namespace Bader.Edge.ModuleHost;

/// <summary>
/// Defines a desired property changed handler and optionally if it should be specific to a desired property.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DesiredPropertiesChangedHandlerAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DesiredPropertiesChangedHandlerAttribute"/> class. This will handle all desired property updates.
    /// </summary>
    public DesiredPropertiesChangedHandlerAttribute() : this(TimeSpan.FromSeconds(30))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DesiredPropertiesChangedHandlerAttribute"/> class. This will handle all desired property updates.
    /// </summary>
    /// <param name="timeout">The timeout of the desired properties changed handler. Default is 30 seconds.</param>
    public DesiredPropertiesChangedHandlerAttribute(TimeSpan timeout) => Timeout = timeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="DesiredPropertiesChangedHandlerAttribute"/> class. This will handle the desired property update of the specified property.
    /// </summary>
    /// <param name="propertyName">The property name of the desired property.</param>
    public DesiredPropertiesChangedHandlerAttribute(string propertyName) : this(propertyName, TimeSpan.FromSeconds(30))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DesiredPropertiesChangedHandlerAttribute"/> class. This will handle the desired property update of the specified property.
    /// </summary>
    /// <param name="propertyName">The property name of the desired property.</param>
    /// <param name="timeout">The timeout of the desired properties changed handler. Default is 30 seconds.</param>
    public DesiredPropertiesChangedHandlerAttribute(string propertyName, TimeSpan timeout)
    {
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        Timeout = timeout;
    }

    /// <summary>
    /// Gets the property name of the desired property. If null is handles all desired property updates.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// Gets the timeout of the desired properties changed handler. The default is 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; }
}
