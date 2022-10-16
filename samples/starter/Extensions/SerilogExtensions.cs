using Serilog.Configuration;
using Serilog.Events;

namespace Serilog
{
    public static class SerilogExtensions
    {
        public static LoggerConfiguration IsRuntimeLogLevel(this LoggerMinimumLevelConfiguration configuration)
        {
            var runtimeLogLevel = Environment.GetEnvironmentVariable("RuntimeLogLevel") ?? "information";
            if (runtimeLogLevel == "info")
            {
                runtimeLogLevel = "information";
            }

            var logLevel = Enum.Parse<LogEventLevel>(runtimeLogLevel, true);
            return configuration.Is(logLevel);
        }
    }
}
