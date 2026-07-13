using Microsoft.Extensions.Logging;
using Polly;

namespace BusinessLogicLayer.Policies
{
    public class UsersMicroservicePolicies : IUsersMicroservicePolicies
    {
        private readonly ILogger<UsersMicroservicePolicies> _logger;

        public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger)
        {
            _logger = logger;
        }

        public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // if request fails, wait and retry...
                .WaitAndRetryAsync(
                    retryCount: 3, // max number of retries
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // exponential backoff
                    onRetry: (outcome, timespan, retryAttempt, context) => // extra actions to perform on retry
                    {
                        _logger.LogWarning("Retry {RetryAttempt} encountered an error: {Error}. Waiting {Delay} before next retry.", retryAttempt, outcome.Exception?.Message, timespan);
                    }
                );
        }

        public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // if request fails, break the circuit...
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5, // number of failures before breaking the circuit
                    durationOfBreak: TimeSpan.FromSeconds(30), // duration to keep the circuit open
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

        public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(2500));
        }

        public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
        {
            var retryPolicy = GetRetryPolicy();
            var circuitBreakerPolicy = GetCircuitBreakerPolicy();
            var timeoutPolicy = GetTimeoutPolicy();

            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        }
    }
}
