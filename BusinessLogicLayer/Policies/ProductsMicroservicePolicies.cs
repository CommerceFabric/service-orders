using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly;

namespace BusinessLogicLayer.Policies
{
    public class ProductsMicroservicePolicies : IProductsMicroservicePolicies
    {
        private readonly ILogger<ProductsMicroservicePolicies> _logger;

        public ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger)
        {
            _logger = logger;
        }

        public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy()
        {
            return Policy.BulkheadAsync<HttpResponseMessage>(
                maxParallelization: 10, // maximum number of concurrent executions
                maxQueuingActions: 20, // maximum number of actions that can be queued
                onBulkheadRejectedAsync: async context => // any additional actions to perform when the bulkhead rejects execution due to too many concurrent requests
                {
                    _logger.LogWarning("Bulkhead rejected execution due to too many concurrent requests.");
                    await Task.CompletedTask;
                }
            );
        }

        public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
        {
            return Policy.Handle<HttpRequestException>() // if the request fails due to a network error, apply fallback logic
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // or if request fails, apply fallback logic (effectively, intercepting the failed response and replacing it)...
                .FallbackAsync(
                    (async (context) =>
                    {
                        _logger.LogWarning("Fallback executed");

                        // fallback data to return when the request fails
                        var product = new ProductDTO
                        (
                            Guid.Empty,
                            "Error retrieving product",
                            "Error retrieving product",
                            0,
                            0
                        );

                        // store the fallback product in the context so it can be accessed later if needed
                        return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(product))
                        };
                    })
                );
        }

        public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
        {
            var fallbackPolicy = GetFallbackPolicy();
            var bulkheadPolicy = GetBulkheadIsolationPolicy();
            return Policy.WrapAsync(fallbackPolicy, bulkheadPolicy); // wrap the policies so that the fallback policy is applied first, followed by the bulkhead isolation policy
        }
    }
}
