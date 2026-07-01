using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO
{
    public record OrderAddRequest(
        Guid UserID,
        DateTime OrderDate,
        List<OrderItemAddRequest> OrderItems
    );
}
