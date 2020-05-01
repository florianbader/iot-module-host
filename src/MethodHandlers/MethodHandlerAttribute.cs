using System;

namespace Bader.Edge.ModuleHost
{
    /// <summary>
    /// Defines a method handler and declares optionally if it's a default method handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MethodHandlerAttribute : Attribute
    {
        /// <summary>
        /// Gets a value indicating whether this handler handles all methods which are not otherwise handled.
        /// </summary>
        public bool IsDefault { get; }

        /// <summary>
        /// Gets the method name of the handled method.
        /// </summary>
        public string MethodName { get; }

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
        public MethodHandlerAttribute(bool isDefault)
        {
            IsDefault = isDefault;
            MethodName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodHandlerAttribute"/> class.
        /// </summary>
        /// <param name="methodName">THe name of the method which should be handled.</param>
        public MethodHandlerAttribute(string methodName) => MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
    }
}
