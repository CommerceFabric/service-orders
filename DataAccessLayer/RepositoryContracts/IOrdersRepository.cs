using DataAccessLayer.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.RepositoryContracts
{
    public interface IOrdersRepository
    {
        /// <summary>
        /// Retrieves all orders from the data source.
        /// </summary>
        /// <returns>A collection of orders.</returns>
        Task<IEnumerable<Order?>> GetOrders();

        /// <summary>
        /// Retrieves a single order based on a specified filter condition.
        /// </summary>
        /// <param name="filter">The filter condition to apply.</param>
        /// <returns>The order that matches the filter condition, or null if no match is found.</returns>
        Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter);

        /// <summary>
        /// Adds a new order to the data source.
        /// </summary>
        /// <param name="order">The order to add.</param>
        /// <returns>The added order, or null if the operation failed.</returns>
        Task<Order?> AddOrder(Order order);

        /// <summary>
        /// Updates an existing order in the data source.
        /// </summary>
        /// <param name="order">The order to update.</param>
        /// <returns>The updated order, or null if the operation failed.</returns>
        Task<Order?> UpdateOrder(Order order);

        /// <summary>
        /// Deletes an order from the data source based on the specified order ID.
        /// </summary>
        /// <param name="orderID">The ID of the order to delete.</param>
        /// <returns>True if the order was successfully deleted; otherwise, false.</returns>
        Task<bool> DeleteOrder(Guid orderID);
    }
}
