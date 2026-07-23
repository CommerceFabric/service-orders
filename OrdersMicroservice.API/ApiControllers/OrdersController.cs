using BusinessLogicLayer.DTO;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OrdersMicroservice.API.ApiControllers
{
    // note: as this is a controller, we do not need to manually use the fluentvalidation library to validate the request, as the controller will automatically validate the request's DTO(s) and return a 400 Bad Request response if the request is invalid.

    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        #region Dependencies
        private readonly IOrdersService _ordersService;
        #endregion

        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        /// <summary>
        /// GET: /api/Orders
        /// ---
        /// Get all orders
        /// </summary>
        /// <returns>All orders</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse?>>> Get()
        {
            var orders = await _ordersService.GetOrders();
            return Ok(orders);
        }

        /// <summary>
        /// GET: /api/Orders/search/orderid/{orderID}
        /// ---
        /// Get a single order by its orderID
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns>A single order matching the specified orderID</returns>
        [HttpGet("search/orderid/{orderID}")]
        public async Task<ActionResult<OrderResponse?>> GetOrderByOrderID(Guid orderID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(o => o.OrderID, orderID);
            var order = await _ordersService.GetOrderByCondition(filter);
            return Ok(order);
        }

        /// <summary>
        /// GET: /api/Orders/search/productID/{productID}
        /// ---
        /// Get all orders matching a specific productID
        /// </summary>
        /// <param name="productID">The ID of the product to search for</param>
        /// <returns>All orders containing the specified productID</returns>
        [HttpGet("search/productid/{productID}")]
        public async Task<ActionResult<IEnumerable<OrderResponse?>>> GetOrderByProductID(Guid productID)
        {
            FilterDefinition<Order> filter = Builders<Order>.Filter.ElemMatch(o => o.OrderItems, oi => oi.ProductID == productID);
            var orders = await _ordersService.GetOrdersByCondition(filter);
            return Ok(orders);
        }

        /// <summary>
        /// GET: /api/Orders/search/userid/{userID}
        /// ---
        /// Get all orders matching a specific userID
        /// </summary>
        /// <param name="userID">The ID of the user to search for</param>
        /// <returns>All orders containing the specified userID</returns>
        [HttpGet("search/userid/{userID}")]
        public async Task<ActionResult<IEnumerable<OrderResponse?>>> GetOrderByUserID(Guid userID)
        {
            // todo - add security so only the user with the specified userID can access their own orders, or an admin can access any user's orders. For now, this is left open for testing purposes.
            FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(o => o.UserID, userID);
            var orders = await _ordersService.GetOrdersByCondition(filter);
            return Ok(orders);
        }

        /// <summary>
        /// GET: /api/Orders/search/orderDate/{orderDate}
        /// ---
        /// Get all orders matching a specific orderDate
        /// </summary>
        /// <param name="orderDate">The date of the orders to search for</param>
        /// <returns>All orders containing the specified orderDate</returns>
        [HttpGet("search/orderdate/{orderDate}")]
        public async Task<ActionResult<IEnumerable<OrderResponse?>>> GetOrderByOrderDate(DateTime orderDate)
        {
            // normalize to utc
            var start = DateTime.SpecifyKind(orderDate.Date, DateTimeKind.Utc);
            var end = start.AddDays(1);

            // only check date, not time, so we need to create a filter that checks if the orderDate is between the start and end of the specified date
            FilterDefinition<Order> filter = Builders<Order>.Filter.And(
                Builders<Order>.Filter.Gte(o => o.OrderDate, start),
                Builders<Order>.Filter.Lt(o => o.OrderDate, end)
            );
            var orders = await _ordersService.GetOrdersByCondition(filter);
            if (orders == null || !orders.Any()) return NotFound();

            return Ok(orders);
        }

        /// <summary>
        /// POST: /api/Orders
        /// ---
        /// Add a new order
        /// </summary>
        /// <param name="order">The order to add</param>
        /// <returns>The added order</returns>
        [HttpPost]
        public async Task<ActionResult<OrderResponse?>> AddOrder(OrderAddRequest order)
        {
            if (order == null) return BadRequest("Order request cannot be null.");

            var addedOrder = await _ordersService.AddOrder(order);
            if (addedOrder == null) return Problem("Failed to add order.");

            return Created($"api/Orders/{addedOrder.OrderID}", addedOrder);
        }

        /// <summary>
        /// PUT: /api/Orders
        /// ---
        /// Update an existing order
        /// </summary>
        /// <param name="orderID">The ID of the order to update</param>
        /// <param name="order">The order to update</param>
        /// <returns>The updated order</returns>
        [HttpPut("{orderID}")]
        public async Task<ActionResult<OrderResponse?>> UpdateOrder(Guid orderID, OrderUpdateRequest order)
        {
            if (order == null) return BadRequest("Order request cannot be null.");
            if (orderID == Guid.Empty) return BadRequest("Order ID in the URL cannot be empty.");
            if (orderID != order.OrderID) return BadRequest("Order ID in the URL does not match the Order ID in the request body.");

            var updatedOrder = await _ordersService.UpdateOrder(order);
            if (updatedOrder == null) return NotFound($"Order with ID {order.OrderID} not found.");

            return Ok(updatedOrder);
        }


        /// <summary>
        /// DELETE: /api/Orders
        /// ---
        /// Delete an existing order
        /// </summary>
        /// <param name="orderID">The ID of the order to delete</param>
        /// <returns>The deleted order</returns>
        [HttpDelete("{orderID}")]
        public async Task<ActionResult> DeleteOrder(Guid orderID)
        {
            if (orderID == Guid.Empty) return BadRequest("Order ID in the URL cannot be empty.");

            var deletedOrder = await _ordersService.DeleteOrder(orderID);
            if (!deletedOrder) return NotFound($"Order with ID {orderID} not found.");

            return NoContent();
        }
    }
}
