using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bader.Edge.ModuleHost.Tests;

public class ModuleHostBuilderTests
{
    [Fact]
    public async Task StartingHost_ShouldOpenModuleClientConnection()
    {
        var moduleClientMock = new Mock<IModuleClient>();
        var startupMock = new Mock<IStartup>();

        startupMock.Setup(s => s.ConfigureServices(It.IsAny<IServiceCollection>()))
            .Callback<IServiceCollection>(s => s.AddSingleton(moduleClientMock.Object));

        using var host = new ModuleHostBuilder()
            .UseStartup(startupMock.Object)
            .ConfigureLogging(logging => logging.AddConsole())
            .Build();

        await host.StartAsync().ConfigureAwait(false);

        moduleClientMock.Verify(m => m.OpenAsync(), Times.Once);
    }
}
