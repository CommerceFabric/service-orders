using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLogicLayer.ServiceContracts
{
    public interface IOrdersService
    {
        /// <summary>
        /// Gets all orders from the database.
        /// </summary>
        /// <returns>A list of order responses.</returns>
        Task<List<OrderResponse?>> GetOrders();

        /// <summary>
        /// Gets an order by its ID.
        /// </summary>
        /// <param name="filter">The filter to apply to the order query.</param>
        /// <returns>The order response if found; otherwise, null.</returns>
        Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter);

        /// <summary>
        /// Gets orders based on a specified filter condition.
        /// </summary>
        /// <param name="filter">The filter to apply to the order query.</param>
        /// <returns>A list of order responses that match the filter condition.</returns>
        Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter);

        /// <summary>
        /// Adds a new order to the database.
        /// </summary>
        /// <param name="orderAddRequest">The request DTO containing the order details.</param>
        /// <returns>The added order response if successful; otherwise, null.</returns>
        Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest);
        /// <summary>
        /// Updates an existing order in the database.
        /// </summary>
        /// <param name="orderUpdateRequest">The request DTO containing the updated order details.</param>
        /// <returns>The updated order response if successful; otherwise, null.</returns>
        Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest);
        /// <summary>
        /// Deletes an order from the database.
        /// </summary>
        /// <param name="orderID">The ID of the order to delete.</param>
        /// <returns>True if the order was deleted successfully; otherwise, false.</returns>
        Task<bool> DeleteOrder(Guid orderID);
    }
}
