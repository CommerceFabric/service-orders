using BusinessLogicLayer.DTO;
using DnsClient.Internal;
using Microsoft.Extensions.Caching.Distributed;
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
        private readonly IDistributedCache _distributedCache;
        #endregion

        public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger, IDistributedCache distributedCache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _distributedCache = distributedCache;
        }

        public async Task<ProductDTO> GetProductByProductID(Guid productID)
        {
            try
            {
                #region check redis cache for product by productID
                var cacheKey = $"product:{productID}"; // create a cache key based on the productID
                var cachedProduct = await _distributedCache.GetStringAsync(cacheKey); // try to get the product from cache

                // if cache hit, return the product from cache
                if (!string.IsNullOrEmpty(cachedProduct))
                    return System.Text.Json.JsonSerializer.Deserialize<ProductDTO>(cachedProduct)!;
                #endregion

                #region if cache miss, call the products microservice to get the product by productID

                // call the products microservice to get the product by productID
                var response = await _httpClient.GetAsync($"api/products/search/product-id/{productID}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null; // if the product is not found, return null
                else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) return await response.Content.ReadFromJsonAsync<ProductDTO>(); // if the products microservice is unavailable, return the fallback default product polly replaces its response with but skip caching
                else if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Error retrieving product with ID {productID}: {response.ReasonPhrase}"); // if any other error occurs, throw an exception
                var product = await response.Content.ReadFromJsonAsync<ProductDTO>();

                // store the product in the cache for future requests
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30), // set the cache expiration time (after this time, the cache entry will be removed)
                    SlidingExpiration = TimeSpan.FromSeconds(10) // set the sliding expiration time (if the product is accessed again within this time, the cache expiration will be extended)
                };
                var serializedProduct = System.Text.Json.JsonSerializer.Serialize(product); // serialize the product to a string
                await _distributedCache.SetStringAsync(cacheKey, serializedProduct, cacheOptions); // store the serialized product in the cache with the cache key and options

                return product!;
                #endregion
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
