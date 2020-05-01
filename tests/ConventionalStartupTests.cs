using System;
using System.Linq;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bader.Edge.ModuleHost.Tests
{
    public class ConventionalStartupTests
    {
        [Fact]
        public void CheckIfAllIStartupMethodsAreCalled()
        {
            var methods = typeof(IStartup).GetMethods();

            var mock = new Mock<IStartup>();

            var startup = new ConventionalStartup(typeof(IStartup), mock.Object);

            foreach (var method in methods)
            {
                var startupMethod = startup.GetType().GetMethod(method.Name);
                startupMethod.Should().NotBeNull($"ConventionalStartup is missing method {method.Name}");

                startupMethod!.Invoke(startup, new object[method.GetParameters().Length]);
                mock.Invocations.Any(i => string.Equals(i.Method.Name, method.Name, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
            }
        }
    }
}
