using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQProductNameUpdateConsumer : IDisposable, IRabbitMQProductNameUpdateConsumer
    {
        private readonly IConfiguration _configuration;
        private IChannel? _channel;
        private IConnection? _connection;
        private readonly SemaphoreSlim _lock = new(1, 1); // Semaphore to ensure thread safety when creating the channel (has 1 permit, so only one thread can enter at a time)

        public RabbitMQProductNameUpdateConsumer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }

        public async Task Consume()
        {
            await EnsureConnectedAsync(); // Ensure that the channel is created and connected to RabbitMQ before consuming the message (has been lazy loaded)

            string routingKey = "product.update.name"; // the routing key that they publisher uses for name updates
            var exchangeName = _configuration["RABBITMQ_PRODUCTS_EXCHANGE"]!; // the name of the exchange to declare (eg products.exchange)

            // Declare the exchange
            await _channel!.ExchangeDeclareAsync(
                exchange: exchangeName, // the name of the exchange to declare (eg products.exchange)
                type: ExchangeType.Direct, // the type of the exchange (eg direct, fanout, topic, headers)
                durable: true // exchange should survive a broker restart
            );

            // Declare the queue
            string queueName = "orders.product.update.name.queue"; // the name of the queue to declare (eg syntax = <nameOfService>.<exchangeItIsConsuming>.queue)
            await _channel!.QueueDeclareAsync(
                queue: queueName, // the name of the queue to declare
                durable: true, // queue should survive a broker restart
                exclusive: false, // queue can be accessed by other connections
                autoDelete: false // queue should not be deleted when the last consumer unsubscribes
            );

            // Bind the queue to the exchange with the routing key
            await _channel!.QueueBindAsync(
                queue: queueName, // the name of the queue to bind
                exchange: exchangeName, // the name of the exchange to bind to
                routingKey: routingKey // the routing key to use for binding
            );
        }

        /// <summary>
        /// Lazy initialization of the RabbitMQ channel. If the channel is already created, it returns immediately. Otherwise, it creates a new connection and channel to RabbitMQ.
        /// Required as it needs async methods to create the connection and channel, and we want to avoid creating them in the constructor.
        /// So instead, we create them on demand when the first message is published.
        /// Afterwhich, the channel is reused for subsequent messages.
        /// </summary>
        /// <returns></returns>
        private async Task EnsureConnectedAsync()
        {
            if (_channel != null) // if the channel is already created, return immediately (we have already lazy initialized the connection and channel for rabbitMQ)
                return;

            await _lock.WaitAsync(); // Use a semaphore to ensure that only one thread can create the connection and channel at a time (so that we don't create multiple connections and channels if multiple threads call Publish at the same time prior to the channel being initialized)


            try
            {
                if (_channel != null) // sanity check to see if the channel was created while waiting for the lock, if so, return immediately
                    return;

                // Create a new connection and channel to RabbitMQ
                var factory = new ConnectionFactory
                {
                    HostName = _configuration["RABBITMQ_HOST"]!,
                    UserName = _configuration["RABBITMQ_USER"]!,
                    Password = _configuration["RABBITMQ_PASSWORD"]!,
                    Port = int.Parse(_configuration["RABBITMQ_PORT"]!)
                };
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
            }
            finally
            {
                _lock.Release(); // Release the semaphore so that other threads can enter and use the channel
            }
        }
    }
}
