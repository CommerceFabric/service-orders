using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.RabbitMQ
{
    public record ProductDeleteMessage
    {
        public Guid ProductID { get; init; }
    }
}
