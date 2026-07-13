using Polly;

namespace BusinessLogicLayer.Policies
{
    public interface IUsersMicroservicePolicies
    {
        /// <summary>
        /// This policy provides a retry mechanism for handling transient failures in HTTP requests.
        /// So if a request to the users microservice fails due to a temporary issue (like a network glitch), this policy will automatically retry the request a specified number of times with an exponential backoff strategy.
        /// </summary>
        /// <returns></returns>
        IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();

        /// <summary>
        /// This policy provides a circuit breaker mechanism for handling repeated failures in HTTP requests.
        /// So if a request to the users microservice fails repeatedly, this policy will "open" the circuit and prevent further requests for a specified duration, allowing the system to recover before attempting to send requests again.
        /// </summary>
        /// <returns></returns>
        IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy();

        /// <summary>
        /// This policy provides a timeout mechanism for handling long-running HTTP requests.
        /// </summary>
        /// <returns></returns>
        IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy();

        /// <summary>
        /// This method combines the retry, circuit breaker, and timeout policies into a single policy.
        /// </summary>
        /// <returns></returns>
        IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
    }
}
