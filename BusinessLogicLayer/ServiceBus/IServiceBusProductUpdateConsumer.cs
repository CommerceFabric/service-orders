using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.ServiceBus
{
    public interface IServiceBusProductUpdateConsumer : IDisposable
    {
        Task ConsumeAsync();
    }
}
