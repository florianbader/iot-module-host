using System.Text;
using Bader.Edge.ModuleHost.Mqtt;
using Microsoft.Azure.Devices.Client;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
    .CreateLogger();

var logger = new SerilogLoggerProvider(Log.Logger).CreateLogger(nameof(Program));

using var moduleClient = new MqttModuleClientBuilder("localDevice", "mymodule", new Uri("mqtt://localhost:1883"))
    .WithLogger(logger)
    .Build();

await moduleClient.OpenAsync();

await moduleClient.SendEventAsync(new Message(Encoding.UTF8.GetBytes("Hello, Cloud!")));

Console.ReadLine();
