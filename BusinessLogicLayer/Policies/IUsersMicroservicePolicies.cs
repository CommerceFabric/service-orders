using Polly;

namespace BusinessLogicLayer.Policies
{
    public interface IUsersMicroservicePolicies
    {
        /// <summary>
        /// This method combines the retry, circuit breaker, and timeout policies into a single policy.
        /// </summary>
        /// <returns></returns>
        IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
    }
}
