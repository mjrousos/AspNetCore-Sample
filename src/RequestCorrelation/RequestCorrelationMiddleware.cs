// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RequestCorrelation
{
    // Middleware: This custom middleware component makes sure that an X-Correlation-Id header is set.
    //             It uses an existing correlation ID if one is present or generates a new one, otherwise
    //             This ID is made available to loggers and can be used to associate separate requests
    //             which combine to form a complete customer action end-to-end.
    public class RequestCorrelationMiddleware
    {
        public const string CorrelationHeaderName = "X-Correlation-ID";
        private readonly RequestDelegate _next;

        // Middleware: Custom middleware types need to have a constructor taking a RequestDelegate argument
        //             that is used to call the next piece of the middleware pipeline. We store it for later.
        //
        // Middleware: This constructor is called once at app startup, so any arguments provided by dependency injection
        //             will be scoped for the lifetime of the application. If DI-provided objects should be scoped more
        //             narrowly, they can be included in the Invoke method's signature.
        public RequestCorrelationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // Middleware: Custom middleware needs an Invoke method that is called when requests come in for processing.
        //             It should have an HttpContext as its first argument and, optionally, other services that are
        //             needed from DI. Dependencies which are injected at this level will be scoped to the lifetime of
        //             the request.
        public async Task Invoke(HttpContext context, ILogger<RequestCorrelationMiddleware> logger)
        {
            // Check for an existing correlation header
            if (!context.Request.Headers.Any(h => h.Key.Equals(CorrelationHeaderName, StringComparison.OrdinalIgnoreCase)))
            {
                // If no correlation header exists, add one. Add to the request (as well as the response) in case future middleware wants to
                // make use of the header.
                var correlationId = Guid.NewGuid().ToString();
                logger.LogInformation($"Request has no correlation header. Adding new correlation ID: {correlationId}");
                context.Request.Headers.Add(CorrelationHeaderName, new StringValues(correlationId));
            }

            var correlationHeader = context.Request.Headers.First(h => h.Key.Equals(CorrelationHeaderName, StringComparison.OrdinalIgnoreCase));
            logger.LogInformation($"Request correlation ID: {correlationHeader.Value.First()}");

            // Propagate the correlation header from the request to the response
            context.Response.Headers.Add(correlationHeader);

            // Add the correlation ID in the logger's scope so that it will be included in subsequent logging events for the HTTP request
            using (logger.BeginScope("CorrelationId:{CorrelationId}", correlationHeader.Value.First()))
            {
                // Call the next piece of middleware in the pipeline
                await _next.Invoke(context);

                // If further logging, etc. needed to happen after subsequent middleware had processed the request, that
                // could be included here.
            }
        }
    }
}
