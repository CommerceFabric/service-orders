namespace BusinessLogicLayer.RabbitMQ.ProductUpdateConsumption
{
    public interface IRabbitMQProductNameUpdateConsumer
    {
        Task Consume();
        void Dispose();
    }
}