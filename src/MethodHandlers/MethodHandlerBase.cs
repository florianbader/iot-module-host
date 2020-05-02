using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;

namespace Bader.Edge.ModuleHost
{
    /// <summary>
    /// This defines a method handler class which catches exceptions and logs them.
    /// </summary>
    public abstract class MethodHandlerBase : IMethodHandler
    {
        private readonly string _name;

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodHandlerBase"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        protected MethodHandlerBase(ILogger logger)
        {
            Logger = logger;
            _name = GetType().FullName;
        }

        /// <inheritdoc />
        async Task<MethodResponse> IMethodHandler.HandleMethodAsync(MethodRequest methodRequest)
        {
            try
            {
                return await HandleMethodAsync(methodRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred handling method in method handler {Name}", _name);
                return Error($"An error occurred handling method: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns a method response for a bad request.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        protected MethodResponse BadRequest(string errorMessage)
            => new MethodResponse(Encoding.UTF8.GetBytes(errorMessage), (int)HttpStatusCode.BadRequest);

        /// <summary>
        /// Returns a method response for an error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        protected MethodResponse Error(string errorMessage)
            => new MethodResponse(Encoding.UTF8.GetBytes(errorMessage), (int)HttpStatusCode.InternalServerError);

        /// <summary>
        /// Handles a method from the cloud or another module.
        /// </summary>
        /// <param name="methodRequest">The method request.</param>
        /// <returns>The method response.</returns>
        protected abstract Task<MethodResponse> HandleMethodAsync(MethodRequest methodRequest);

        /// <summary>
        /// Returns a method response for a success.
        /// </summary>
        protected MethodResponse Ok() => new MethodResponse((int)HttpStatusCode.OK);
    }
}
