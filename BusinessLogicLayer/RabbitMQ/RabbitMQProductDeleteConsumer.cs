using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace BusinessLogicLayer.RabbitMQ
{
    public class RabbitMQProductDeleteConsumer : IDisposable, IRabbitMQProductDeleteConsumer
    {
        #region Dependencies
        private readonly IConfiguration _configuration;
        private readonly ILogger<RabbitMQProductDeleteConsumer> _logger;
        private readonly IDistributedCache _distributedCache;
        #endregion

        private IChannel? _channel;
        private IConnection? _connection;
        private readonly SemaphoreSlim _lock = new(1, 1); // Semaphore to ensure thread safety when creating the channel (has 1 permit, so only one thread can enter at a time)

        public RabbitMQProductDeleteConsumer(IConfiguration configuration, ILogger<RabbitMQProductDeleteConsumer> logger, IDistributedCache distributedCache)
        {
            _configuration = configuration;
            _logger = logger;
            _distributedCache = distributedCache;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }

        public async Task Consume()
        {
            await EnsureConnectedAsync(); // Ensure that the channel is created and connected to RabbitMQ before consuming the message (has been lazy loaded)

            #region Declare Exchange, Queue, and Bindings
            var headers = new Dictionary<string, object>
            {
                {"x-match", "all" }, // all headers must match for the message to be routed to the queue
                { "event", "product.delete" },
                { "RowCount", 1  }
            };
            var exchangeName = _configuration["RABBITMQ_PRODUCTS_EXCHANGE"]!; // the name of the exchange to declare (eg products.exchange)

            // Declare the exchange
            await _channel!.ExchangeDeclareAsync(
                exchange: exchangeName, // the name of the exchange to declare (eg products.exchange)
                type: ExchangeType.Headers, // the type of the exchange (eg direct, fanout, topic, headers)
                durable: true // exchange should survive a broker restart
            );

            // Declare the queue
            string queueName = "orders.product.delete.queue"; // the name of the queue to declare (eg syntax = <nameOfService>.<exchangeItIsConsuming>.queue)
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
                routingKey: string.Empty, // the routing key to use for binding (not needed for headers exchange, but still required by the method signature)
                arguments: headers! // the headers to use for binding
            );
            #endregion

            #region Handle Message Consumption Events
            // Create a new consumer to handle the message delivery confirmation
            var consumer = new AsyncEventingBasicConsumer(_channel!);

            // Define the event handler for when a message is received
            AsyncEventHandler<BasicDeliverEventArgs> asyncEventHandler = async (sender, args) =>
            {
                var body = args.Body.ToArray(); // get the message body that was published to RabbitMQ as a byte array
                var message = Encoding.UTF8.GetString(body); // convert the byte array to a string using UTF-8 encoding

                var productDeleteMessage = System.Text.Json.JsonSerializer.Deserialize<ProductDeleteMessage>(message); // deserialize the message to a ProductDeleteMessage object
                if (productDeleteMessage == null) return;

                try
                {   
                    #region update the redis cache
                    // handle potential stale Redis cache for the product delete
                    var cacheKey = $"product:{productDeleteMessage?.ProductID}"; // create a cache key based on the productID
                    await _distributedCache.RemoveAsync(cacheKey); // invalidate the stale cache (if it exists) by removing the cache key from Redis
                    #endregion

                    _logger.LogInformation($"RabbitMQ Consumer received delete message, invalidated cache for: ProductID: {productDeleteMessage?.ProductID}");

                    await _channel.BasicAckAsync(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    // todo - should probably modify to something like: <retry 3 times -> add to dead letter exchange + some dead letter queue for special handling)
                    _logger.LogError(ex, $"Failed processing RabbitMQ delete message for ProductID: {productDeleteMessage?.ProductID}");

                    await _channel.BasicNackAsync(args.DeliveryTag, false, true);
                }
            };
            consumer!.ReceivedAsync += asyncEventHandler;

            // Start consuming messages from the queue with the specified routing key and consumer
            await _channel!.BasicConsumeAsync(
                queue: queueName, // the name of the queue to consume from
                autoAck: false, // automatically acknowledge the message delivery
                consumer: consumer // the consumer to handle the message delivery confirmation
            );
            #endregion
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
