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

        public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy()
        {
            return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // if request fails, apply fallback logic (effectively, intercepting the failed response and replacing it)...
                .FallbackAsync(
                    (async(context) =>
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
                        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                        {
                            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(product))
                        };
                    })
                );
        }
    }
}
