using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Driver;

namespace BusinessLogicLayer.Services
{
    public class OrdersService : IOrdersService
    {
        #region Dependenceies
        private readonly IOrdersRepository _ordersRepository;
        private readonly IMapper _mapper;
        private readonly UsersMicroserviceClient _usersMicroserviceClient;
        private readonly ProductsMicroserviceClient _productsMicroserviceClient;


        #region Validators
        // Validators are injected via DI and can be executed in two different ways:
        // 1. At the API boundary (controllers), as FluentValidation is integrated with ASP.NET Core using AddFluentValidationAutoValidation(), validation runs automatically during model binding and invalid requests never reach the controller action.
        // 2. But in the business layer (this service), validation must be executed manually unless a pipeline mechanism (e.g., MediatR pipeline behaviors) is used.
        private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
        private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
        private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
        private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
        #endregion
        #endregion


        public OrdersService(IOrdersRepository ordersRepository, 
            IMapper mapper,
            UsersMicroserviceClient usersMicroserviceClient,
            ProductsMicroserviceClient productsMicroserviceClient,
            IValidator<OrderAddRequest> orderAddRequestValidator, 
            IValidator<OrderUpdateRequest> orderUpdateRequestValidator, 
            IValidator<OrderItemAddRequest> orderItemAddRequestValidator, 
            IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator)
        {
            _ordersRepository = ordersRepository;
            _mapper = mapper;
            _usersMicroserviceClient = usersMicroserviceClient;
            _productsMicroserviceClient = productsMicroserviceClient;

            _orderAddRequestValidator = orderAddRequestValidator;
            _orderUpdateRequestValidator = orderUpdateRequestValidator;
            _orderItemAddRequestValidator = orderItemAddRequestValidator;
            _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        }

        public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
        {
            #region Validation
            // check if orderAddRequest is null, if it is throw an ArgumentNullException
            if (orderAddRequest == null) throw new ArgumentNullException(nameof(orderAddRequest));

            // check the validity of the orderAddRequest using the validator, if it is not valid throw a ValidationException with the error messages
            await ValidateAsync(_orderAddRequestValidator, orderAddRequest);

            // check the validity of each order item in the orderAddRequest using the validator, if any of them is not valid throw a ValidationException with the error messages
            foreach (var orderItem in orderAddRequest.OrderItems)
            {
                await ValidateAsync(_orderItemAddRequestValidator, orderItem);

                // Check against products microservice that the product exists
                var productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItem.ProductID);
                if (productDTO == null) throw new ArgumentException($"Product with ID {orderItem.ProductID} not found.");
            }

            // Check against users microservice that the user exists
            var userDTO = await _usersMicroserviceClient.GetUserByUserID(orderAddRequest.UserID);
            if(userDTO == null) throw new ArgumentException($"User with ID {orderAddRequest.UserID} not found.");
            #endregion

            // Convert data from orderAddRequest to Order entity
            var orderInput = _mapper.Map<Order>(orderAddRequest);

            // Generate values
            foreach(OrderItem orderItem in orderInput.OrderItems)
            {
                orderItem._id = Guid.NewGuid();
                orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
            }

            // Invoke repository method to add the order to the database
            var orderOutput = await _ordersRepository.AddOrder(orderInput);

            // Convert data from Order entity to OrderResponse DTO
            var orderResponse = _mapper.Map<OrderResponse>(orderOutput);
            return orderResponse;
        }

        public async Task<bool> DeleteOrder(Guid orderID)
        {
            #region Validation
            if (orderID == Guid.Empty) throw new ArgumentException("OrderID cannot be empty.", nameof(orderID));
            var filter = Builders<Order>.Filter.Eq(o => o.OrderID, orderID);
            var existingOrder = await _ordersRepository.GetOrderByCondition(filter);
            if (existingOrder == null) throw new KeyNotFoundException($"Order with ID {orderID} not found.");
            #endregion

            // Invoke repository method to delete the order from the database
            var result = await _ordersRepository.DeleteOrder(orderID);
            return result;
        }

        public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
        {
            var order = await _ordersRepository.GetOrderByCondition(filter);
            return _mapper.Map<OrderResponse>(order);
        }

        public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
        {
            var orders = await _ordersRepository.GetOrdersByCondition(filter);
            return _mapper.Map<IEnumerable<OrderResponse?>>(orders).ToList();
        }

        public async Task<List<OrderResponse?>> GetOrders()
        {
            var orders = await _ordersRepository.GetOrders();
            return _mapper.Map<IEnumerable<OrderResponse?>>(orders).ToList();
        }

        public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
        {
            #region Validation
            // check if orderUpdateRequest is null, if it is throw an ArgumentNullException
            if (orderUpdateRequest == null) throw new ArgumentNullException(nameof(orderUpdateRequest));

            // check the validity of the orderUpdateRequest using the validator, if it is not valid throw a ValidationException with the error messages
            await ValidateAsync(_orderUpdateRequestValidator, orderUpdateRequest);

            // check the validity of each order item in the orderUpdateRequest using the validator, if any of them is not valid throw a ValidationException with the error messages
            foreach (var orderItem in orderUpdateRequest.OrderItems)
            {
                await ValidateAsync(_orderItemUpdateRequestValidator, orderItem);

                // Check against products microservice that the product exists
                var productDTO = await _productsMicroserviceClient.GetProductByProductID(orderItem.ProductID);
                if(productDTO == null) throw new ArgumentException($"Product with ID {orderItem.ProductID} not found.");
            }

            // Check against users microservice that the user exists
            var userDTO = await _usersMicroserviceClient.GetUserByUserID(orderUpdateRequest.UserID);
            if (userDTO == null) throw new ArgumentException($"User with ID {orderUpdateRequest.UserID} not found.");
            #endregion

            // Convert data from orderUpdateRequest to Order entity
            var orderInput = _mapper.Map<Order>(orderUpdateRequest);

            // Generate values
            foreach (OrderItem orderItem in orderInput.OrderItems)
            {
                orderItem._id = Guid.NewGuid();
                orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
            }

            // Invoke repository method to update the order in the database
            var orderOutput = await _ordersRepository.UpdateOrder(orderInput);

            // Convert data from Order entity to OrderResponse DTO
            var orderResponse = _mapper.Map<OrderResponse>(orderOutput);
            return orderResponse;
        }

        #region Helper Methods
        /// <summary>
        /// Validates the given model using the provided validator. If the validation fails, a ValidationException is thrown with the error messages.
        /// </summary>
        /// <typeparam name="T">The type of the model to validate.</typeparam>
        /// <param name="validator">The validator to use for validation.</param>
        /// <param name="model">The model to validate.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ValidationException">Thrown when the validation fails.</exception>
        private static async Task ValidateAsync<T>(IValidator<T> validator, T model)
        {
            var validation = await validator.ValidateAsync(model);

            if (!validation.IsValid)
            {
                throw new ValidationException(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));
            }
        }
        #endregion
    }
}
