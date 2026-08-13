using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.ServiceBus.ProductUpdateConsumption
{
    public interface IServiceBusProductUpdateConsumer : IDisposable
    {
        Task ConsumeAsync();
    }
}
