using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using MongoDB.Driver;
using Testcontainers.MongoDb;
using Xunit;

namespace OrdersServiceTests.DataAccessLayer.Repositories
{
    public class OrdersRepositoryTests : IAsyncLifetime
    {
        private readonly MongoDbContainer _mongoContainer;

        private IMongoDatabase _database = null!;
        private OrdersRepository _repository = null!;

        public OrdersRepositoryTests()
        {
            _mongoContainer = new MongoDbBuilder()
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _mongoContainer.StartAsync();

            var client = new MongoClient(_mongoContainer.GetConnectionString());

            _database = client.GetDatabase("OrdersTestDb");

            _repository = new OrdersRepository(_database);
        }

        public async Task DisposeAsync()
        {
            await _mongoContainer.DisposeAsync();
        }

        [Fact]
        public async Task AddOrder_ShouldAddOrder()
        {
            // Arrange
            var order = new Order
            {
                UserID = Guid.NewGuid(),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem()
                }
            };

            // Act
            var result = await _repository.AddOrder(order);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.OrderID);
            Assert.Equal(result.OrderID, result._id);

            Assert.All(result.OrderItems, item =>
            {
                Assert.NotEqual(Guid.Empty, item._id);
            });
        }

        [Fact]
        public async Task GetOrderByCondition_ShouldReturnOrder()
        {
            // Arrange
            var order = new Order
            {
                UserID = Guid.NewGuid(),
                OrderItems = new List<OrderItem>()
            };

            await _repository.AddOrder(order);

            var filter = Builders<Order>.Filter.Eq(o => o.OrderID, order.OrderID);

            // Act
            var result = await _repository.GetOrderByCondition(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.OrderID, result.OrderID);
        }

        [Fact]
        public async Task GetOrderByCondition_ShouldReturnNull_WhenNotFound()
        {
            var filter = Builders<Order>.Filter.Eq(o => o.OrderID, Guid.NewGuid());

            var result = await _repository.GetOrderByCondition(filter);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrders_ShouldReturnAllOrders()
        {
            // Arrange
            await _repository.AddOrder(new Order
            {
                UserID = Guid.NewGuid(),
                OrderItems = new List<OrderItem>()
            });

            await _repository.AddOrder(new Order
            {
                UserID = Guid.NewGuid(),
                OrderItems = new List<OrderItem>()
            });

            // Act
            var result = await _repository.GetOrders();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetOrdersByCondition_ShouldReturnMatchingOrders()
        {
            // Arrange
            var UserID = Guid.NewGuid();

            await _repository.AddOrder(new Order
            {
                UserID = UserID,
                OrderItems = new List<OrderItem>()
            });

            await _repository.AddOrder(new Order
            {
                UserID = Guid.NewGuid(),
                OrderItems = new List<OrderItem>()
            });

            var filter = Builders<Order>.Filter.Eq(o => o.UserID, UserID);

            // Act
            var result = await _repository.GetOrdersByCondition(filter);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task DeleteOrder_ShouldDeleteExistingOrder()
        {
            // Arrange
            var order = new Order
            {
                UserID = Guid.NewGuid(),
                OrderItems = new List<OrderItem>()
            };

            await _repository.AddOrder(order);

            // Act
            var result = await _repository.DeleteOrder(order.OrderID);

            // Assert
            Assert.True(result);

            var filter = Builders<Order>.Filter.Eq(o => o.OrderID, order.OrderID);
            var deleted = await _repository.GetOrderByCondition(filter);

            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteOrder_ShouldReturnFalse_WhenOrderDoesNotExist()
        {
            var result = await _repository.DeleteOrder(Guid.NewGuid());

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateOrder_ShouldUpdateExistingOrder()
        {
            // Arrange
            var order = new Order
            {
                UserID = Guid.NewGuid(),
                OrderItems = new List<OrderItem>()
            };

            await _repository.AddOrder(order);

            var newUserID = Guid.NewGuid();
            order.UserID = newUserID;

            // Act
            var result = await _repository.UpdateOrder(order);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newUserID, result.UserID);
        }

        [Fact]
        public async Task UpdateOrder_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            var order = new Order
            {
                OrderID = Guid.NewGuid(),
                UserID = Guid.NewGuid(),
                OrderItems = new List<OrderItem>()
            };

            var result = await _repository.UpdateOrder(order);

            Assert.Null(result);
        }
    }
}