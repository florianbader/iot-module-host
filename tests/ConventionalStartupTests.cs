using System;
using System.Linq;
using System.Reflection;
using AIT.Devices;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace tests
{
    public class ConventionalStartupTests
    {
        [Fact]
        public void CheckIfAllIStartupMethodsAreCalled()
        {
            var methods = typeof(IStartup).GetMethods();

            var mock = new Mock<IStartup>();
            var obj = mock.Object;

            var startup = new ConventionalStartup(typeof(IStartup), mock.Object);

            foreach (var method in methods)
            {
                var startupMethod = startup.GetType().GetMethod(method.Name);
                startupMethod.Should().NotBeNull($"ConventionalStartup is missing method {method.Name}");

                startupMethod.Invoke(startup, new object[method.GetParameters().Length]);
                mock.Invocations.Any(i => string.Compare(i.Method.Name, method.Name) == 0).Should().BeTrue();
            }
        }
    }
}
