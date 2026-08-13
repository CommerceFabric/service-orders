namespace BusinessLogicLayer.RabbitMQ.ProductDeleteConsumption
{
    public interface IRabbitMQProductDeleteConsumer
    {
        Task Consume();
        void Dispose();
    }
}