using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.ServiceBus.Publisher
{
    public interface IServiceBusPublisher
    {
        Task Publish<T>(string topicName, Dictionary<string, object> headers, T message);
    }
}
