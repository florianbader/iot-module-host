using System;

namespace Bader.Edge.ModuleHost
{
    internal class SystemTime : ISystemTime
    {
        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;
    }
}
