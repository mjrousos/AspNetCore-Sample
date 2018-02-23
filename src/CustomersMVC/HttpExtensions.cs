// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using System.Net.Http;

namespace CustomersMVC
{
    public static class HttpExtensions
    {
        // 5xx status codes indicate a server-side error
        public static bool ReportsServerError(this HttpResponseMessage response) => ((int)response.StatusCode) / 100 == 5;
    }
}
