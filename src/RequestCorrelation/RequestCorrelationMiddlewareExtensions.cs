// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using RequestCorrelation;

namespace Microsoft.AspNetCore.Builder
{
    // Middleware: Middleware often have helper extension methods on IAppcaliation builder
    //             to wrap the call to UseMiddleware in a friendlier method.
    //
    // Middleware: Note that IApplicationBuilder extension methods typically live in the
    //             Microsoft.AspNetCore.Builder namespace so that they can be easily referenced
    //             by a developer using IApplicationBuilder.
    public static class RequestCorrelationMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestCorrelation(this IApplicationBuilder app) => app.UseMiddleware<RequestCorrelationMiddleware>();
    }
}
