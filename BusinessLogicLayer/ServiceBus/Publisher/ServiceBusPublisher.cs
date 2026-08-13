using Azure.Messaging.ServiceBus;
using BusinessLogicLayer.ServiceBus.Publisher;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace BusinessLogicLayer.ServiceBus.Publisher
{
    public class ServiceBusPublisher : IServiceBusPublisher
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly IConfiguration _configuration;

        private ServiceBusSender _serviceBusSender;

        public ServiceBusPublisher(ServiceBusClient serviceBusClient, IConfiguration configuration)
        {
            _serviceBusClient = serviceBusClient;
            _configuration = configuration;
        }

        public async Task Publish<T>(string topicName, Dictionary<string, object> headers, T message)
        {
            _serviceBusSender = _serviceBusClient.CreateSender(topicName);

            // create serialized service bus message 
            var messageJson = JsonSerializer.Serialize(message);
            var serviceBusMessage = new ServiceBusMessage(messageJson);

            // add headers to the service bus message
            foreach (var header in headers)
            {
                serviceBusMessage.ApplicationProperties[header.Key] = header.Value;
            }

            // send the message to the service bus topic
            await _serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}
