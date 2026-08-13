using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.ServiceBus
{
    public interface IServiceBusProductDeleteConsumer : IDisposable
    {
        Task ConsumeAsync();
    }
}
