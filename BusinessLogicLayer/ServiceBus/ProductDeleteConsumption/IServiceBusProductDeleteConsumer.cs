using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.ServiceBus.ProductDeleteConsumption
{
    public interface IServiceBusProductDeleteConsumer : IDisposable
    {
        Task ConsumeAsync();
    }
}
