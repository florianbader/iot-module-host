using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace Bader.Edge.ModuleHost
{
    /// <summary>
    /// This defines a method handler class.
    /// </summary>
    public interface IMethodHandler
    {
        /// <summary>
        /// Handles a method from the cloud or another module.
        /// </summary>
        /// <param name="methodRequest">The method request.</param>
        /// <returns>The method response.</returns>
        Task<MethodResponse> HandleMethodAsync(MethodRequest methodRequest);
    }
}
