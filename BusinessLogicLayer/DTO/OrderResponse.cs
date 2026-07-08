using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.DTO
{
    public record OrderResponse
    {
        public Guid UserID { get; set; }
        public Guid OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalBill { get; set; }
        public List<OrderItemResponse>? OrderItems { get; set; }
        public string? UserPersonName { get; set; }
        public string? UserEmail { get; set; }
    }
}
