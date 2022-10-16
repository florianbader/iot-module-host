namespace Bader.Edge.ModuleHost;

internal interface ISystemTime
{
    DateTime Now { get; }

    DateTime UtcNow { get; }
}
