using System;

namespace Bader.Edge.ModuleHost
{
    /// <summary>
    /// Defines a message handler and declares optionally if it's a default message handler or for a specific input.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class MessageHandlerAttribute : Attribute
    {
        /// <summary>
        /// Gets the input name of the handled message.
        /// </summary>
        public string InputName { get; }

        /// <summary>
        /// Gets a value indicating whether this handler handles all messages which are not otherwise handled.
        /// </summary>
        public bool IsDefault { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlerAttribute"/> class.
        /// </summary>
        public MessageHandlerAttribute()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlerAttribute"/> class specifying if it's the default handler.
        /// </summary>
        /// <param name="isDefault">Whether this handler handles all messages which are not otherwise handled.</param>
        public MessageHandlerAttribute(bool isDefault)
        {
            IsDefault = isDefault;
            InputName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandlerAttribute"/> class specifying if the name of the message input in the route.
        /// </summary>
        /// <param name="inputName">The input name of the handled method.</param>
        public MessageHandlerAttribute(string inputName) => InputName = inputName ?? throw new ArgumentNullException(nameof(inputName));
    }
}
