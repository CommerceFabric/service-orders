using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO
{
    public record OrderResponse(
        Guid UserID,
        Guid OrderID,
        DateTime OrderDate,
        decimal TotalBill,
        List<OrderItemResponse> OrderItems
    );
}
