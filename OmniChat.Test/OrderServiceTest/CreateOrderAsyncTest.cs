using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Order;
using OmniChat.Infrastructure.Dtos.Requests.OrderItem;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Claim = System.Security.Claims.Claim;

namespace OmniChat.Test.OrderServiceTest;

public class CreateOrderAsyncTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<OrderService>> _loggerMock = new();
    protected readonly Mock<ICreditNoteService> _creditNoteMock = new();
    protected readonly Mock<IMailService> _mailServiceMock = new();
    private readonly Mock<IProductBatchAuditService> _auditServiceMock = new();

    private OrderService CreateService()
    {
        return new OrderService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _creditNoteMock.Object,
            _mailServiceMock.Object,
            _auditServiceMock.Object
        );
    }

    protected Mock<IGenericRepository<T>> SetupRepository<T>() where T : class
    {
        var repoMock = new Mock<IGenericRepository<T>>();

        _uowMock.Setup(x => x.GetRepository<T>())
            .Returns(repoMock.Object);

        return repoMock;
    }

    protected void SetupTransaction()
    {
        _uowMock
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());
    }
    private void SetupHttpContext(Guid userId)
    {
        var claims = new List<Claim>
    {
        new Claim("UserId", userId.ToString())
    };

        var identity = new ClaimsIdentity(claims, "mock");

        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = principal
        };

        _httpMock.Setup(x => x.HttpContext)
            .Returns(context);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCreateOrder_AndDeductBatchQuantity()
    {
        var orderRepo = SetupRepository<Order>();
        var batchRepo = SetupRepository<ProductBatch>();
        var staffRepo = SetupRepository<Staff>();

        SetupTransaction();
        SetupHttpContext(Guid.NewGuid());

        var batchId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var batch = new ProductBatch
        {
            Id = batchId,
            Quantity = 10,
            Product = new Product
            {
                Quantity = 100,
                Price = 20
            }
        };

        var order = new Order
        {
            OrderItems = new List<OrderItem>()
        };

        _mapperMock
            .Setup(m => m.Map<Order>(It.IsAny<CreateOrderRequest>()))
            .Returns(order);

        var batches = new List<ProductBatch> { batch };

        var queryable = new TestAsyncEnumerable<ProductBatch>(batches);

        batchRepo.Setup(r => r.GetQueryable(
            It.IsAny<Expression<Func<ProductBatch, bool>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IQueryable<ProductBatch>>>(),
            It.IsAny<bool>()))
        .Returns(queryable);

        staffRepo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Staff, bool>>>(),
            It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
            It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
        .ReturnsAsync(new Staff
        {
            Id = Guid.NewGuid(),
            AccountId = accountId
        });

        var service = CreateService();

        var request = new CreateOrderRequest
        {
            OrderItems = new List<AddOrderItemRequest>
        {
            new AddOrderItemRequest
            {
                ProductBatchId = batchId,
                Quantity = 2
            }
        }
        };

        var result = await service.CreateOrderAsync(request);

        Assert.True(result);

        Assert.Equal(8, batch.Quantity);
        Assert.Equal(98, batch.Product.Quantity);

        Assert.Single(order.OrderItems);

        Assert.Equal(40, order.TotalAmount);

        orderRepo.Verify(r =>
            r.InsertAsync(It.IsAny<Order>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrowNotFound_WhenBatchMissing()
    {
        var orderRepo = SetupRepository<Order>();
        var batchRepo = SetupRepository<ProductBatch>();
        var staffRepo = SetupRepository<Staff>();

        SetupTransaction();
        SetupHttpContext(Guid.NewGuid());

        var batchId = Guid.NewGuid();

        batchRepo.Setup(r => r.GetQueryable(
            It.IsAny<Expression<Func<ProductBatch, bool>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IQueryable<ProductBatch>>>(),
            It.IsAny<bool>()))
        .Returns(new TestAsyncEnumerable<ProductBatch>(new List<ProductBatch>()));

        var service = CreateService();

        var request = new CreateOrderRequest
        {
            OrderItems = new List<AddOrderItemRequest>
        {
            new AddOrderItemRequest
            {
                ProductBatchId = batchId,
                Quantity = 1
            }
        }
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateOrderAsync(request));
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldThrowBusinessException_WhenInsufficientStock()
    {
        var orderRepo = SetupRepository<Order>();
        var batchRepo = SetupRepository<ProductBatch>();
        var staffRepo = SetupRepository<Staff>();

        SetupTransaction();
        SetupHttpContext(Guid.NewGuid());

        var batchId = Guid.NewGuid();

        var batch = new ProductBatch
        {
            Id = batchId,
            Quantity = 1,
            Product = new Product
            {
                Quantity = 100,
                Price = 20
            }
        };

        batchRepo.Setup(r => r.GetQueryable(
            It.IsAny<Expression<Func<ProductBatch, bool>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IQueryable<ProductBatch>>>(),
            It.IsAny<bool>()))
        .Returns(new TestAsyncEnumerable<ProductBatch>(new List<ProductBatch> { batch }));

        var service = CreateService();

        var request = new CreateOrderRequest
        {
            OrderItems = new List<AddOrderItemRequest>
        {
            new AddOrderItemRequest
            {
                ProductBatchId = batchId,
                Quantity = 5
            }
        }
        };

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateOrderAsync(request));
    }
}
