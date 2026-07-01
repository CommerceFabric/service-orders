using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO
{
    public record OrderUpdateRequest(
        Guid UserID,
        Guid OrderID,
        DateTime OrderDate,
        List<OrderItemUpdateRequest> OrderItems
    );
}
