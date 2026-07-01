using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        #region dependencies
        private readonly IMongoDatabase _mongoDatabase;
        #endregion

        private readonly string _collectionName = "Orders";
        IMongoCollection<Order> _ordersCollection;

        public OrdersRepository(IMongoDatabase mongoDatabase) 
        {
            _mongoDatabase = mongoDatabase;

            _ordersCollection = _mongoDatabase.GetCollection<Order>(_collectionName);
        }

        public async Task<Order?> AddOrder(Order order)
        {
            order.OrderID = Guid.NewGuid(); // Generate a new OrderID for the order
            order._id = order.OrderID;

            foreach (var item in order.OrderItems)
            {
                item._id = Guid.NewGuid(); // Generate a new ItemID for each item
            }

            await _ordersCollection.InsertOneAsync(order);
            return order;
        }

        public async Task<bool> DeleteOrder(Guid orderID)
        {
            var filter = Builders<Order>.Filter.Eq(o => o.OrderID, orderID); // must use OrderID, not _id, as _id is the MongoDB internal ID and OrderID is the business ID
            
            var existingOrder = await _ordersCollection.Find(filter).FirstOrDefaultAsync();
            if(existingOrder == null) return false; // Order not found, cannot delete

            var result = await _ordersCollection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
        {
            var orders = await _ordersCollection.FindAsync(filter);
            return await orders.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
        {
            var orders = await _ordersCollection.FindAsync(filter);
            return await orders.ToListAsync();
        }

        public async Task<IEnumerable<Order?>> GetOrders()
        {
            var orders = await _ordersCollection.FindAsync(_ => true); // Get all orders, always true
            return await orders.ToListAsync();
        }

        public async Task<Order?> UpdateOrder(Order order)
        {
            order.OrderID = Guid.NewGuid();
            var filter = Builders<Order>.Filter.Eq(o => o.OrderID, order.OrderID); // must use OrderID, not _id, as _id is the MongoDB internal ID and OrderID is the business ID

            var existingOrder = await _ordersCollection.Find(filter).FirstOrDefaultAsync();
            if(existingOrder == null) return null; // Order not found, cannot update

            var result = await _ordersCollection.ReplaceOneAsync(filter, order);
            return result.IsAcknowledged ? order : null;
        }
    }
}
