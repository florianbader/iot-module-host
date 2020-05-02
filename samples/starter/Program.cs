using System;
using System.IO;
using System.Threading.Tasks;
using AIT.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Exceptions;

namespace Starter
{
    public static class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            // To initialize the module host use the ModuleHostBuilder class.
            // Because this class implements the IHostBuilder interface you can also use other extensions.
            new ModuleHostBuilder(args)
                // The only option you need to set is the Startup class which should be used.
                // You don't need to specify method handlers or message handlers they are found through their interface.
                .UseStartup<Startup>()
                .UseSerilog();

        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithExceptionDetails()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                await CreateHostBuilder(args).RunConsoleAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error occurred on startup");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
