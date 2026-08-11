using Azure.Messaging.ServiceBus;
using BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.ServiceBus
{
    public class ServiceBusProductUpdateConsumer : IServiceBusProductUpdateConsumer
    {
        private readonly ServiceBusProcessor _serviceBusProcessor;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<ServiceBusProductUpdateConsumer> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IConfiguration _configuration;

        public ServiceBusProductUpdateConsumer(IDistributedCache distributedCache, ILogger<ServiceBusProductUpdateConsumer> logger, ServiceBusClient serviceBusClient, IConfiguration configuration)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _serviceBusClient = serviceBusClient;
            _configuration = configuration;

            // instantiate the ServiceBusProcessor with the topic and subscription from configuration
            var topic = _configuration["ProductsServiceBus:ProductTopic"];
            var subscription = _configuration["ProductsServiceBus:ProductOrdersSubscription"];
            var options = new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false, // We will manually complete the messages after processing
            };
            _serviceBusProcessor = _serviceBusClient.CreateProcessor(topic, subscription, options);

            _serviceBusProcessor.ProcessMessageAsync += _serviceBusProcessor_ProcessMessageAsync;
            _serviceBusProcessor.ProcessErrorAsync += _serviceBusProcessor_ProcessErrorAsync;
        }

        #region ProductUpdate Message Handling To Update Cache
        private async Task _serviceBusProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var messageBodyJson = arg.Message.Body.ToString();
            var productDTO = JsonSerializer.Deserialize<ProductDTO>(messageBodyJson);

            if (productDTO != null)
            {
                await HandleProductUpdate(productDTO);
            }

            await arg.CompleteMessageAsync(arg.Message); // tell Service Bus that the message has been processed successfully
        }

        private async Task HandleProductUpdate(ProductDTO updatedProduct)
        {
            try
            {
                #region update the redis cache
                // handle potential stale Redis cache for the product name update
                var cacheKey = $"product:{updatedProduct?.ProductID}"; // create a cache key based on the productID
                var serializedProduct = System.Text.Json.JsonSerializer.Serialize(updatedProduct); // serialize the updated cached product to a string

                // define the cache options for the updated cached product, including the absolute expiration time and sliding expiration time
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30), // set the cache expiration time (after this time, the cache entry will be removed)
                };
                await _distributedCache.SetStringAsync(cacheKey, serializedProduct, cacheOptions); // store the serialized product in the cache with the cache key and options     
                #endregion

                _logger.LogInformation($"Service Bus Consumer received update message, updated cache for: ProductID: {updatedProduct?.ProductID} ({updatedProduct?.ProductName})");
            }
            catch (Exception ex)
            {
                // todo - should probably modify to something like: <retry 3 times -> add to dead letter exchange + some dead letter queue for special handling)
                _logger.LogError(ex, $"Failed processing Service Bus update message for ProductID: {updatedProduct?.ProductID} ({updatedProduct?.ProductName})");
            }
        }
        #endregion

        private Task _serviceBusProcessor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            throw new NotImplementedException();
        }

        public async Task ConsumeAsync()
        {
            await _serviceBusProcessor.StartProcessingAsync();
        }

        public async void Dispose()
        {
            await _serviceBusProcessor.StopProcessingAsync();
            await _serviceBusProcessor.DisposeAsync();
        }
    }
}
