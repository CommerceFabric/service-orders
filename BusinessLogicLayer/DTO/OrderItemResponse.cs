using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO
{
    public record OrderItemResponse(
        Guid ProductID,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice
    );
}
