using System;

namespace AIT.Devices
{
    public class MethodHandlerAttribute : Attribute
    {
        public MethodHandlerAttribute()
        {
        }

        public MethodHandlerAttribute(bool isDefault)
        {
            IsDefault = isDefault;
        }

        public MethodHandlerAttribute(string methodName)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
        }

        public string MethodName { get; }

        public bool IsDefault { get; }
    }
}