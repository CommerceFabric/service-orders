using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Polly;
using System.Threading.Tasks;

namespace ApiGateway.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class OcelotClientIdFallbackMiddleware
    {
        private readonly RequestDelegate _next;

        public OcelotClientIdFallbackMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware to check for the presence of the "ClientId" header in the incoming HTTP request. 
        /// If the header is missing, it generates a fallback client identifier based on the client's IP address and adds it to the request headers. 
        /// The request is then passed to the next middleware in the pipeline.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext httpContext)
        {
            const string clientIdHeader = "ClientId";

            if (!httpContext.Request.Headers.ContainsKey(clientIdHeader))
            {
                var clientId = GetClientIdentifier(httpContext);

                httpContext.Request.Headers[clientIdHeader] = clientId;
            }

            await _next(httpContext);
        }

        /// <summary>
        /// Generates a fallback client identifier based on the client's IP address. If the IP address is not available, it returns "anonymous" as the identifier.
        /// </summary>
        /// <param name="context">The HTTP context of the current request.</param>
        /// <returns>A string representing the client identifier.</returns>
        private static string GetClientIdentifier(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress;

            if (ipAddress != null)
            {
                return ipAddress.ToString();
            }

            return "anonymous";
        }
    }

    /// <summary>
    /// Extension methods for adding the OcelotClientIdFallbackMiddleware to the ASP.NET Core middleware pipeline.
    /// </summary>
    public static class OcelotClientIdFallbackMiddlewareExtensions
    {
        /// <summary>
        /// Adds the OcelotClientIdFallbackMiddleware to the ASP.NET Core middleware pipeline. 
        /// This middleware checks for the presence of the "ClientId" header in incoming requests and, if not provided, uses a fallback identifier of the client's IP address (or anonymous if not found).
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseOcelotClientIdFallbackMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OcelotClientIdFallbackMiddleware>();
        }
    }
}
