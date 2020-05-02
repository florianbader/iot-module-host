using System;

namespace Bader.Edge.ModuleHost
{
    public interface ISystemTime
    {
        DateTime Now => DateTime.Now;

        DateTime UtcNow => DateTime.UtcNow;
    }
}
