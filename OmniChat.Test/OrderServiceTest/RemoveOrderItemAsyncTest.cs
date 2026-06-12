using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Claim = System.Security.Claims.Claim;

namespace OmniChat.Test.OrderServiceTest;

public class RemoveOrderItemAsyncTest
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    private readonly Mock<ILogger<OrderService>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IHttpContextAccessor> _httpMock = new();
    private readonly Mock<ICreditNoteService> _creditNoteMock = new();
    private readonly Mock<IMailService> _mailServiceMock = new();
    private readonly Mock<IProductBatchAuditService> _auditServiceMock = new(); private readonly Mock<IWalletService> _walletServiceMock = new();
    private string _userId = Guid.NewGuid().ToString();
    private OrderService CreateService()
    {
        return new OrderService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _creditNoteMock.Object,
            _mailServiceMock.Object,
            _auditServiceMock.Object,
            _walletServiceMock.Object
        );
    }

    private Mock<IGenericRepository<Order>> SetupOrderRepo()
    {
        var repo = new Mock<IGenericRepository<Order>>();
        _uowMock.Setup(x => x.GetRepository<Order>()).Returns(repo.Object);
        return repo;
    }

    private Mock<IGenericRepository<ProductBatch>> SetupBatchRepo()
    {
        var repo = new Mock<IGenericRepository<ProductBatch>>();
        _uowMock.Setup(x => x.GetRepository<ProductBatch>()).Returns(repo.Object);
        return repo;
    }

    private void SetupTransaction()
    {
        _uowMock.Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var identity = new ClaimsIdentity(new[]
    {
            new Claim("UserId", _userId)
        }, "Test");

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        _httpMock.Setup(x => x.HttpContext).Returns(httpContext);
    }
    private Mock<IGenericRepository<Staff>> SetupStaffRepo()
    {
        var repo = new Mock<IGenericRepository<Staff>>();
        _uowMock.Setup(x => x.GetRepository<Staff>()).Returns(repo.Object);

        repo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
                It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
            .ReturnsAsync(new Staff
            {
                Id = Guid.NewGuid(),
                AccountId = Guid.Parse(_userId)
            });

        return repo;
    }
    private static IQueryable<T> AsyncQueryable<T>(List<T> data)
        => new TestAsyncEnumerable<T>(data);

    [Fact]
    public async Task RemoveOrderItem_ShouldRestoreStock_AndRemoveItem()
    {
        var orderRepo = SetupOrderRepo();
        var batchRepo = SetupBatchRepo();
        SetupTransaction();
        SetupStaffRepo();
        var batchId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        var product = new Product { Quantity = 10 };

        var batch = new ProductBatch
        {
            Id = batchId,
            Quantity = 5,
            Product = product
        };

        var orderItem = new OrderItem
        {
            Id = orderItemId,
            ProductBatchId = batchId,
            Quantity = 2,
            Price = 100
        };

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderItems = new List<OrderItem> { orderItem }
        };

        orderRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<bool>()))
            .Returns(AsyncQueryable(new List<Order> { order }));

        batchRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<ProductBatch, bool>>>(),
                It.IsAny<Func<IQueryable<ProductBatch>, IQueryable<ProductBatch>>>(),
                It.IsAny<bool>()))
            .Returns(AsyncQueryable(new List<ProductBatch> { batch }));

        var service = CreateService();

        var result = await service.RemoveOrderItemAsync(order.Id, orderItemId);

        Assert.True(result);
        Assert.Empty(order.OrderItems);
        Assert.Equal(7, batch.Quantity);
        Assert.Equal(12, product.Quantity);
    }

    [Fact]
    public async Task RemoveOrderItem_ShouldThrow_WhenOrderNotFound()
    {
        var orderRepo = SetupOrderRepo();
        var batchRepo = SetupBatchRepo();
        SetupTransaction();

        orderRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<bool>()))
            .Returns(AsyncQueryable(new List<Order>()));

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.RemoveOrderItemAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveOrderItem_ShouldThrow_WhenOrderItemMissing()
    {
        var orderRepo = SetupOrderRepo();
        var batchRepo = SetupBatchRepo();
        SetupTransaction();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderItems = new List<OrderItem>() 
        };

        orderRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<bool>()))
            .Returns(AsyncQueryable(new List<Order> { order }));

        var service = CreateService();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RemoveOrderItemAsync(order.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveOrderItem_ShouldThrow_WhenBatchMissing()
    {
        var orderRepo = SetupOrderRepo();
        var batchRepo = SetupBatchRepo();
        SetupTransaction();

        var batchId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        var orderItem = new OrderItem
        {
            Id = orderItemId,
            ProductBatchId = batchId,
            Quantity = 2,
            Price = 100
        };

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderItems = new List<OrderItem> { orderItem }
        };

        orderRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<bool>()))
            .Returns(AsyncQueryable(new List<Order> { order }));

        batchRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<ProductBatch, bool>>>(),
                It.IsAny<Func<IQueryable<ProductBatch>, IQueryable<ProductBatch>>>(),
                It.IsAny<bool>()))
            .Returns(AsyncQueryable(new List<ProductBatch>()));

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.RemoveOrderItemAsync(order.Id, orderItemId));
    }
}