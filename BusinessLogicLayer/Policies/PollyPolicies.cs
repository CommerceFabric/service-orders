using Microsoft.Extensions.Logging;
using Polly;

namespace BusinessLogicLayer.Policies
{
    public class PollyPolicies : IPollyPolicies
    {
        private readonly ILogger<PollyPolicies> _logger;

        public PollyPolicies(ILogger<PollyPolicies> logger)
        {
            _logger = logger;
        }

        public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
        {
            return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // if request fails, wait and retry...
                .WaitAndRetryAsync(
                    retryCount: retryCount, // max number of retries
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // exponential backoff
                    onRetry: (outcome, timespan, retryAttempt, context) => // extra actions to perform on retry
                    {
                        _logger.LogWarning("Retry {RetryAttempt} encountered an error: {Error}. Waiting {Delay} before next retry.", retryAttempt, outcome.Exception?.Message, timespan);
                    }
                );
        }

        public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
        {
            return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // if request fails, break the circuit...
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking, // number of failures before breaking the circuit
                    durationOfBreak: durationOfBreak, // duration to keep the circuit open
                    onBreak: (outcome, timespan) =>
                    {
                        _logger.LogWarning("Circuit breaker opened for {Duration} due to: {Error}", timespan, outcome.Exception?.Message);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset.");
                    }
                );
        }

        public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int milliseconds)
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(milliseconds));
        }
    }
}
