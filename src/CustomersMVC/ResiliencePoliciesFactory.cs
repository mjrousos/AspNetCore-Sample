// Licensed under the MIT license. See LICENSE file in the samples root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Net.Http;

namespace CustomersMVC
{
    public class ResiliencePoliciesFactory
    {
        private readonly ILogger<ResiliencePoliciesFactory> _logger;
        private readonly int _retryCount;
        private readonly int _exceptionsAllowedBeforeCircuitBreak;

        public ResiliencePoliciesFactory(ILogger<ResiliencePoliciesFactory> logger, IConfiguration configuration)
        {
            _logger = logger;

            if (!int.TryParse(configuration["ResilientHttp:RetryCount"], out _retryCount))
            {
                _retryCount = 3;
            }

            if (!int.TryParse(configuration["ResilientHttp:ExceptionsBeforeCircuitBreak"], out _exceptionsAllowedBeforeCircuitBreak))
            {
                _exceptionsAllowedBeforeCircuitBreak = 2;
            }
        }

        /// <summary>
        /// Instantiates Polly resilience policies
        /// </summary>
        /// <returns>A collection of reslience policies</returns>
        internal Policy[] CreatePolicies()
        {
            // Retry failed requests
            var standardHttpRetry = Policy.Handle<HttpRequestException>()

                // Number of times to retry and backoff function
                .WaitAndRetryAsync(_retryCount, i => TimeSpan.FromSeconds(Math.Pow(2, i) * 0.5),
                (exception, waitDuration, retryCount, context) =>
                {
                    // Log warning if retries don't work
                    _logger.LogWarning("Retry {retryCount} after {waitDuration} seconds due to: {exception}",
                        retryCount,
                        waitDuration.TotalSeconds,
                        exception);
                });

            // Stop trying requests that repeatedly fail
            var standardHttpCircuitBreaker = Policy.Handle<HttpRequestException>()
                .CircuitBreakerAsync(_exceptionsAllowedBeforeCircuitBreak, TimeSpan.FromSeconds(60),
                (exception, duration) =>
                {
                    // Log warning when circuit break is opened
                    _logger.LogWarning("Circuit breaker opened for {circuitBreakerDuration} seconds due to: {exception}",
                        duration.TotalSeconds,
                        exception);
                },
                () =>
                {
                    // Log informational message when the circuit breaker resets
                    _logger.LogInformation("Circuit breaker closed");
                },
                () =>
                {
                    // Log informational message when the circuit breaker is half-opem
                    _logger.LogInformation("Circuit half-open");
                });

            // The order of policies matters. When wrapped, the later policies apply first (they are the 'inner' policies)
            // So, in this example, circuit breaker checks are done before retries.
            return new Policy[] { standardHttpRetry, standardHttpCircuitBreaker };
        }
    }
}
