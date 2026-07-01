using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO
{
    public record OrderItemAddRequest(
        Guid ProductID,
        int Quantity,
        decimal UnitPrice
    );
}
