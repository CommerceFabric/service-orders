using Polly;

namespace BusinessLogicLayer.Policies
{
    public interface IProductsMicroservicePolicies
    {
        /// <summary>
        /// This policy provides a fallback mechanism for handling failures in HTTP requests.
        /// For example, if a request to the products microservice fails, this policy can provide a default response or an alternative action to ensure the system continues to function gracefully.
        /// </summary>
        /// <returns></returns>
        IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();

        /// <summary>
        /// This policy prevents the system from being overwhelmed by too many concurrent requests. 
        /// It limits the number of concurrent executions and queues additional requests up to a specified limit. 
        /// If the limit is exceeded, it rejects further requests until some of the current executions complete.
        /// </summary>
        /// <returns></returns>
        IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy();

        /// <summary>
        /// This method combines the fallback and bulkhead isolation policies into a single policy.
        /// </summary>
        /// <returns></returns>
        IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
    }
}
