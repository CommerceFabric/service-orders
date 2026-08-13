using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.RabbitMQ.ProductDeleteConsumption
{
    public class RabbitMQProductDeleteHostedService : IHostedService
    {
        private readonly IRabbitMQProductDeleteConsumer _consumer;

        public RabbitMQProductDeleteHostedService(IRabbitMQProductDeleteConsumer consumer)
        {
            _consumer = consumer;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _consumer.Consume(); // Start consuming messages from RabbitMQ when the hosted service starts
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer.Dispose();
        }
    }
}
