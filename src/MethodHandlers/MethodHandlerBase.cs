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
                Logger.LogTrace("Executing method handler {MethodHandlerName}", _name);

                var response = await HandleMethodAsync(methodRequest).ConfigureAwait(false);

                Logger.LogTrace("Successfully handled method {MethodHandlerName}", _name);

                return response;
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
        /// <returns>A method response with status code 400 and the given error message.</returns>
        protected MethodResponse BadRequest(string errorMessage)
            => new MethodResponse(Encoding.UTF8.GetBytes(errorMessage), (int)HttpStatusCode.BadRequest);

        /// <summary>
        /// Returns a method response for an error.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>A method response with status code 500 and the given error message.</returns>
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
        /// <returns>A method response with status code 200.</returns>
        protected MethodResponse Ok() => new MethodResponse((int)HttpStatusCode.OK);
    }
}
