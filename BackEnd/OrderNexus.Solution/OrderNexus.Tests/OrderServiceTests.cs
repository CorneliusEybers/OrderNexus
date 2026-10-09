using Moq;
using NUnit.Framework;
using OrderNexus.Application.Contracts;
using OrderNexus.Application.Exceptions;
using OrderNexus.Application.Interfaces;
using OrderNexus.Application.Services;
using OrderNexus.Domain.Entities;

namespace OrderNexus.Tests
{
    /// <summary>
    /// Tests the critical order-management rules without requiring a database.
    /// </summary>
    [TestFixture]
    public class OrderServiceTests
    {
        #region Class Variables

        private Mock<IOrderRepository> _orderRepository = null!;

        private OrderService _orderService = null!;

        #endregion

        #region Setup

        [SetUp]
        public void SetUp()
        {
            _orderRepository = new Mock<IOrderRepository>(MockBehavior.Strict);
            _orderService = new OrderService(_orderRepository.Object);
        }

        #endregion

        #region Public Methods

        [Test]
        public void CreateAsync_WhenReferenceIsEmpty_RejectsRequest()
        {
            // - Arrange
            CreateOrderRequest request = CreateRequest(reference: " ");

            // - Act
            BusinessException exception = Assert.ThrowsAsync<BusinessException>(async () => await _orderService.CreateAsync(request))!;

            // - Assert
            Assert.That(exception.StatusCode, Is.EqualTo(400));
            _orderRepository.VerifyNoOtherCalls();
        }

        [Test]
        public void CreateAsync_WhenQuantityIsZero_RejectsRequest()
        {
            // - Arrange
            CreateOrderRequest request = CreateRequest(lines: new List<OrderLineRequest>
            {
                new OrderLineRequest(1, 0)
            });
            
            // - Act
            BusinessException exception = Assert.ThrowsAsync<BusinessException>(async () => await _orderService.CreateAsync(request))!;

            // - Assert
            Assert.That(exception.StatusCode, Is.EqualTo(400));
            _orderRepository.VerifyNoOtherCalls();
        }

        [Test]
        public void CreateAsync_WhenSameItemAppearsTwice_RejectsRequest()
        {
            // - Arrange
            CreateOrderRequest request = CreateRequest(lines: new List<OrderLineRequest>
            {
                new OrderLineRequest(1, 1),
                new OrderLineRequest(1, 2)
            });

            // - Act
            BusinessException exception = Assert.ThrowsAsync<BusinessException>(async () => await _orderService.CreateAsync(request))!;

            // - Assert
            Assert.That(exception.StatusCode, Is.EqualTo(400));
            _orderRepository.VerifyNoOtherCalls();
        }

        [Test]
        public async Task CreateAsync_WhenSubmissionRepeatsIdentically_ReturnsExistingOrderWithoutSaving()
        {
            // - Arrange
            Order existing = CreateExistingOrder();
            CreateOrderRequest request = CreateRequest(reference: " order-100 ");
            ConfigureExistingReference(existing);

            // - Act
            (OrderResponse result, bool created) = await _orderService.CreateAsync(request);

            // - Assert
            Assert.Multiple(() =>
            {
                Assert.That(created, Is.False);
                Assert.That(result.Id, Is.EqualTo(existing.Id));
                Assert.That(result.Total, Is.EqualTo(900m));
            });

            _orderRepository.Verify(repo => repo.Add(It.IsAny<Order>()), Times.Never);
            _orderRepository.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void CreateAsync_WhenReferenceMatchesButQuantityDiffers_ReturnsConflict()
        {
            // - Arrange
            Order existing = CreateExistingOrder();

            CreateOrderRequest request = CreateRequest(lines: new List<OrderLineRequest>
            {
                new OrderLineRequest(1, 3)
            });

            // - Act
            ConfigureExistingReference(existing);

            // - Assert
            BusinessException exception = Assert.ThrowsAsync<BusinessException>(async () => await _orderService.CreateAsync(request))!;

            Assert.That(exception.StatusCode, Is.EqualTo(409));
            _orderRepository.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task CreateAsync_WhenValid_UsesCataloguePriceAndCreatesPendingOrder()
        {
            // - Arrange
            CreateOrderRequest request = CreateRequest();
            Order? addedOrder = null;

            // - Mock
            _orderRepository.Setup(repo => repo.GetByReferenceAsync(1, "ORDER-100", It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Order?)null);

       
            ConfigureValidLookups();

            _orderRepository.Setup(repo => repo.ItemsAsync(It.IsAny<IEnumerable<long>>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new Dictionary<long, Item>{
                                                                        [1] = new Item { Id = 1, SKU = "LS-001", Name = "Laptop Stand", UnitPrice = 450m }
                                                                    });

            _orderRepository.Setup(repo => repo.StatusByCodeAsync("PENDING", It.IsAny<CancellationToken>()))
                                               .ReturnsAsync(new OrderStatus { Id = 1, Code = "PENDING", Name = "Pending" });

            _orderRepository.Setup(repo => repo.Add(It.IsAny<Order>()))
                            .Callback<Order>(ord =>{
                                                       addedOrder = ord;
                                                       ord.Id = 42;
                                                   });

            _orderRepository.Setup(repo => repo.SaveAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            _orderRepository.Setup(repo => repo.GetAsync(42, false, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(() =>
                            {
                                if (addedOrder is null)
                                {
                                    throw new InvalidOperationException("Order was not added.");
                                }
                                HydrateForResponse(addedOrder);
                                return addedOrder;
                            });

            // - Act
            (OrderResponse result, bool created) = await _orderService.CreateAsync(request);

            // - Assert
            Assert.Multiple(() => {
                                      Assert.That(created, Is.True);
                                      Assert.That(result.StatusCode, Is.EqualTo("PENDING"));
                                      Assert.That(result.ExternalReference, Is.EqualTo("ORDER-100"));
                                      Assert.That(result.Items.Single().UnitPrice, Is.EqualTo(450m));
                                      Assert.That(result.Total, Is.EqualTo(900m));
                                  });

            _orderRepository.Verify(repo => repo.Add(It.IsAny<Order>()), Times.Once);
            _orderRepository.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase("PENDING", "CONFIRMED")]
        [TestCase("PENDING", "CANCELLED")]
        [TestCase("CONFIRMED", "FULFILLED")]
        [TestCase("CONFIRMED", "CANCELLED")]
        public async Task ChangeStatusAsync_WhenTransitionAllowed_SavesNewStatus(string source, string destination)
        {
            // - Arrange
            Order order = CreateExistingOrder(source);
            OrderStatus newStatus = new OrderStatus { Id = 9, Code = destination, Name = destination };
            ConfigureOrderLookup(order);

            // - Mock
            _orderRepository.Setup(repo => repo.StatusAsync(9, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(newStatus);

            _orderRepository.Setup(repo => repo.SaveAsync(It.IsAny<CancellationToken>()))
                            .Returns(Task.CompletedTask);

            // - Act
            OrderResponse result = await _orderService.ChangeStatusAsync(order.Id, 9);

            // - Assert
            Assert.That(result.StatusCode, Is.EqualTo(destination));
            _orderRepository.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase("PENDING", "FULFILLED")]
        [TestCase("CONFIRMED", "PENDING")]
        [TestCase("FULFILLED", "CANCELLED")]
        [TestCase("CANCELLED", "CONFIRMED")]
        public void ChangeStatusAsync_WhenTransitionNotAllowed_ReturnsConflict(string source, string destination)
        {
            // - Arrange
            Order order = CreateExistingOrder(source);
            ConfigureOrderLookup(order);

            // - Mock
            _orderRepository.Setup(repo => repo.StatusAsync(9, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new OrderStatus { Id = 9, Code = destination, Name = destination });

            BusinessException exception = Assert.ThrowsAsync<BusinessException>(async () => await _orderService.ChangeStatusAsync(order.Id, 9))!;

            Assert.That(exception.StatusCode, Is.EqualTo(409));
            _orderRepository.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void UpdateAsync_WhenOrderIsConfirmed_ReturnsConflict()
        {
            // - Arrange
            Order order = CreateExistingOrder("CONFIRMED");
            ConfigureOrderLookup(order);
            UpdateOrderRequest request = new UpdateOrderRequest(1, 
                                                                1, 
                                                                1, 
                                                                "ORDER-100", 
                                                                "Sample note", 
                                                                new List<OrderLineRequest>{
                                                                                              new OrderLineRequest(1, 2)
                                                                                          });
            // - Act
            BusinessException exception = Assert.ThrowsAsync<BusinessException>(async () => await _orderService.UpdateAsync(order.Id, request))!;

            // - Assert
            Assert.That(exception.StatusCode, Is.EqualTo(409));
            _orderRepository.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task DeleteAsync_WhenPending_RemovesAndSavesOrder()
        {
            // - Arrange
            Order order = CreateExistingOrder("PENDING");
            ConfigureOrderLookup(order);

            // - Mock
            _orderRepository.Setup(repo => repo.Remove(order));
            _orderRepository.Setup(repo => repo.SaveAsync(It.IsAny<CancellationToken>()))
                                               .Returns(Task.CompletedTask);

            // - Act
            await _orderService.DeleteAsync(order.Id);

            // - Assert
            _orderRepository.Verify(repo => repo.Remove(order), Times.Once);
            _orderRepository.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase("CONFIRMED")]
        [TestCase("FULFILLED")]
        [TestCase("CANCELLED")]
        public void DeleteAsync_WhenNotPending_ReturnsConflict(string status)
        {
            // - Arrange
            Order order = CreateExistingOrder(status);
            ConfigureOrderLookup(order);

            // - Act with Assert
            BusinessException exception = Assert.ThrowsAsync<BusinessException>(async () => await _orderService.DeleteAsync(order.Id))!;

            // - Assert
            Assert.That(exception.StatusCode, Is.EqualTo(409));
            _orderRepository.Verify(repo => repo.Remove(It.IsAny<Order>()), Times.Never);
            _orderRepository.Verify(repo => repo.SaveAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public void GetAsync_WhenOrderDoesNotExist_ReturnsNotFound()
        {
            // - Arrange/Mock
            _orderRepository.Setup(repo => repo.GetAsync(999, false, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((Order?)null);

            // - Act with Assert
            BusinessException exception = Assert.ThrowsAsync<BusinessException>(async () => await _orderService.GetAsync(999))!;

            // - Assert
            Assert.That(exception.StatusCode, Is.EqualTo(404));
        }

        #endregion

        #region Private Methods

        private static CreateOrderRequest CreateRequest(string reference = "ORDER-100",
                                                        List<OrderLineRequest>? lines = null)
        {
            return new CreateOrderRequest(1,
                                          1,
                                          1,
                                          reference,
                                          "Sample note",
                                          lines ?? new List<OrderLineRequest> { new OrderLineRequest(1, 2) });
        }

        private static Order CreateExistingOrder(string statusCode = "PENDING")
        {
            Order order = new Order
            {
                Id = 11,
                CustomerId = 1,
                Customer = new Customer { Id = 1, Name = "Acme Industries" },
                SalesRepId = 1,
                SalesRep = new SalesRep { Id = 1, Name = "James Mitchell" },
                CurrencyId = 1,
                Currency = new Currency { Id = 1, ISOCode = "ZAR", CurrencyName = "South African Rand" },
                OrderStatusId = 1,
                OrderStatus = new OrderStatus { Id = 1, Code = statusCode, Name = statusCode },
                ExternalReference = "ORDER-100",
                Notes = "Sample note"
            };

            order.OrderItems.Add(new OrderItem
            {
                Id = 3,
                OrderId = order.Id,
                ItemId = 1,
                Item = new Item { Id = 1, SKU = "LS-001", Name = "Laptop Stand", UnitPrice = 450m },
                Quantity = 2,
                UnitPrice = 450m
            });
            return order;
        }

        private static void HydrateForResponse(Order order)
        {
            order.Customer = new Customer { Id = 1, Name = "Acme Industries" };
            order.SalesRep = new SalesRep { Id = 1, Name = "James Mitchell" };
            order.Currency = new Currency { Id = 1, ISOCode = "ZAR", CurrencyName = "South African Rand" };
            foreach (OrderItem orditm in order.OrderItems)
            {
                orditm.Item = new Item { Id = orditm.ItemId, SKU = "LS-001", Name = "Laptop Stand", UnitPrice = 450m };
            }
        }

        private void ConfigureExistingReference(Order existing)
        {
            _orderRepository.Setup(repo => repo.GetByReferenceAsync(1, "ORDER-100", It.IsAny<CancellationToken>()))
                            .ReturnsAsync(existing);
        }

        private void ConfigureOrderLookup(Order order)
        {
            _orderRepository.Setup(repo => repo.GetAsync(order.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(order);
        }

        private void ConfigureValidLookups()
        {
            _orderRepository.Setup(repo => repo.CustomerExistsAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _orderRepository.Setup(repo => repo.SalesRepExistsAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _orderRepository.Setup(repo => repo.CurrencyExistsAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
        }

        #endregion
    }
}
