using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AIT.Devices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace tests
{
    public class ModuleHostBuilderTests
    {
        [Fact]
        public async Task StartingHost_ShouldOpenModuleClientConnection()
        {
            var moduleClientMock = new Mock<IModuleClient>();
            var startupMock = new Mock<IStartup>();

            startupMock.Setup(s => s.ConfigureServices(It.IsAny<IServiceCollection>()))
                .Callback<IServiceCollection>(s => s.AddSingleton<IModuleClient>(moduleClientMock.Object));

            await new ModuleHostBuilder()
                .UseStartup(startupMock.Object)
                .ConfigureLogging(logging => logging.AddConsole())
                .Build()
                .StartAsync();

            moduleClientMock.Verify(m => m.OpenAsync(), Times.Once);
        }
    }
}
