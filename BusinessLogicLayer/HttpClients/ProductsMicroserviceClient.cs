using BusinessLogicLayer.DTO;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net.Http.Json;

namespace BusinessLogicLayer.HttpClients
{
    public class ProductsMicroserviceClient
    {
        #region dependencies
        private readonly HttpClient _httpClient;
        private readonly ILogger<ProductsMicroserviceClient> _logger;
        #endregion

        public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ProductDTO> GetProductByProductID(Guid productID)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/products/search/product-id/{productID}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null; // if the product is not found, return null
                else if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Error retrieving product with ID {productID}: {response.ReasonPhrase}"); // if any other error occurs, throw an exception
                var product = await response.Content.ReadFromJsonAsync<ProductDTO>();
                return product;
            }
            catch (BulkheadRejectedException)
            {
                _logger.LogWarning("Bulkhead limit reached. Request rejected.");

                return new ProductDTO
                (
                    Guid.Empty,
                    "Error retrieving product",
                    "Error retrieving product",
                    0,
                    0
                );
            }
        }
    }
}
