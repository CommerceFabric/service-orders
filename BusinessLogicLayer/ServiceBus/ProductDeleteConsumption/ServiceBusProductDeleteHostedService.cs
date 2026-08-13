using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.ServiceBus.ProductDeleteConsumption
{
    public class ServiceBusProductDeleteHostedService : IHostedService
    {
        private readonly IServiceBusProductDeleteConsumer _consumer;

        public ServiceBusProductDeleteHostedService(IServiceBusProductDeleteConsumer consumer)
        {
            _consumer = consumer;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _consumer.ConsumeAsync(); // Start consuming messages from ServiceBus when the hosted service starts
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.Dispose();
        }
    }
}
