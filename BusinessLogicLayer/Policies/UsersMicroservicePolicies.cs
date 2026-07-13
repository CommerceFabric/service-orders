using Microsoft.Extensions.Logging;
using Polly;

namespace BusinessLogicLayer.Policies
{
    public class UsersMicroservicePolicies : IUsersMicroservicePolicies
    {
        private readonly ILogger<UsersMicroservicePolicies> _logger;
        private readonly IPollyPolicies _pollyPolicies;

        public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger, IPollyPolicies pollyPolicies)
        {
            _logger = logger;
            _pollyPolicies = pollyPolicies;
        }

        public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
        {
            var retryPolicy = _pollyPolicies.GetRetryPolicy(3);
            var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(2, TimeSpan.FromMinutes(1));
            var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(5000);

            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);
        }
    }
}
