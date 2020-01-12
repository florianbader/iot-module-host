using System;

namespace AIT.Devices
{
    public class MethodHandlerAttribute : Attribute
    {
        public MethodHandlerAttribute() : this(true)
        {
        }

        public MethodHandlerAttribute(bool isDefault)
        {
            IsDefault = isDefault;
            MethodName = string.Empty;
        }

        public MethodHandlerAttribute(string methodName)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        }

        public string MethodName { get; }

        public bool IsDefault { get; }
    }
}
