using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.Services
{
    public class OrdersService : IOrdersService
    {
        public Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteOrder(Guid orderID)
        {
            throw new NotImplementedException();
        }

        public Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderResponse?>> GetOrders()
        {
            throw new NotImplementedException();
        }

        public Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
        {
            throw new NotImplementedException();
        }
    }
}
