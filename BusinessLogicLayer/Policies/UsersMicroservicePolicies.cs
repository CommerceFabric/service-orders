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
                    retryCount: 5, // max number of retries
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // exponential backoff
                    onRetry: (outcome, timespan, retryAttempt, context) => // extra actions to perform on retry
                    {
                        _logger.LogWarning("Retry {RetryAttempt} encountered an error: {Error}. Waiting {Delay} before next retry.", retryAttempt, outcome.Exception?.Message, timespan);
                    }
                );
        }
    }
}
