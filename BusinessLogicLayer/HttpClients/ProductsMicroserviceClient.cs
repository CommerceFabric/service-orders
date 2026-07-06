using BusinessLogicLayer.DTO;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace BusinessLogicLayer.HttpClients
{
    public class ProductsMicroserviceClient
    {
        #region dependencies
        private readonly HttpClient _httpClient;
        #endregion

        public ProductsMicroserviceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProductResponse> GetProductByProductID(Guid productID)
        {
            var response = await _httpClient.GetAsync($"api/products/{productID}");
            
            if(response.StatusCode == System.Net.HttpStatusCode.NotFound) return null; // if the product is not found, return null
            else if (!response.IsSuccessStatusCode) throw new HttpRequestException($"Error retrieving product with ID {productID}: {response.ReasonPhrase}"); // if any other error occurs, throw an exception

            var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
            return product;
        }
    }
}
