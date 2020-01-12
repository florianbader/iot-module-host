using System;

namespace AIT.Devices
{
    public class MessageHandlerAttribute : Attribute
    {
        public MessageHandlerAttribute() : this(true)
        {
        }

        public MessageHandlerAttribute(bool isDefault)
        {
            IsDefault = isDefault;
            InputName = string.Empty;
        }

        public MessageHandlerAttribute(string inputName)
        {
            InputName = inputName ?? throw new ArgumentNullException(nameof(inputName));
        }

        public string InputName { get; }

        public bool IsDefault { get; }
    }
}