using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.RabbitMQ
{
    public record ProductNameUpdateMessage
    {
        public Guid ProductID { get; init; }
        public string OldProductName { get; init; } = string.Empty;
        public string NewProductName { get; init; } = string.Empty;
    }
}
