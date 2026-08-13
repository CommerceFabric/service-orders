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
    public class ServiceBusProductDeleteConsumer : IServiceBusProductDeleteConsumer
    {
        private readonly ServiceBusProcessor _serviceBusProcessor;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<ServiceBusProductDeleteConsumer> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IConfiguration _configuration;

        public ServiceBusProductDeleteConsumer(IDistributedCache distributedCache, ILogger<ServiceBusProductDeleteConsumer> logger, ServiceBusClient serviceBusClient, IConfiguration configuration)
        {
            _distributedCache = distributedCache;
            _logger = logger;
            _serviceBusClient = serviceBusClient;
            _configuration = configuration;

            // instantiate the ServiceBusProcessor with the topic and subscription from configuration
            var topic = _configuration["ProductsServiceBus:ProductDeleteTopic"];
            var subscription = _configuration["ProductsServiceBus:ProductDeleteOrdersSubscription"];
            var options = new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false, // We will manually complete the messages after processing
            };
            _serviceBusProcessor = _serviceBusClient.CreateProcessor(topic, subscription, options);

            _serviceBusProcessor.ProcessMessageAsync += _serviceBusProcessor_ProcessMessageAsync;
            _serviceBusProcessor.ProcessErrorAsync += _serviceBusProcessor_ProcessErrorAsync;
        }

        #region ProductDelete Message Handling To Delete Cache
        private async Task _serviceBusProcessor_ProcessMessageAsync(ProcessMessageEventArgs arg)
        {
            var messageBodyJson = arg.Message.Body.ToString();
            var productDTO = JsonSerializer.Deserialize<ProductDTO>(messageBodyJson);

            if (productDTO != null)
            {
                await HandleProductDelete(productDTO);
            }

            await arg.CompleteMessageAsync(arg.Message); // tell Service Bus that the message has been processed successfully
        }

        private async Task HandleProductDelete(ProductDTO deletedProduct)
        {
            try
            {
                #region update the redis cache
                // handle potential stale Redis cache for the product delete
                var cacheKey = $"product:{deletedProduct?.ProductID}"; // create a cache key based on the productID
                await _distributedCache.RemoveAsync(cacheKey); // invalidate the stale cache (if it exists) by removing the cache key from Redis
                #endregion

                _logger.LogInformation($"Service Bus Consumer received delete message, invalidated cache for: ProductID: {deletedProduct?.ProductID}");
            }
            catch (Exception ex)
            {
                // todo - should probably modify to something like: <retry 3 times -> add to dead letter exchange + some dead letter queue for special handling)
                _logger.LogError(ex, $"Failed processing Service Bus delete message for ProductID: {deletedProduct?.ProductID} ({deletedProduct?.ProductName})");
            }
        }
        #endregion

        private async Task _serviceBusProcessor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            _logger.LogError(arg.Exception, $"Service Bus Processor encountered an error: {arg.Exception.Message}");
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
