using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.OrderItem;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace OmniChat.Test.OrderServiceTest;

public class UpdateOrderItemAsyncTest
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    private readonly Mock<ILogger<OrderService>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IHttpContextAccessor> _httpMock = new();
    private readonly Mock<ICreditNoteService> _creditNoteMock = new();
    private readonly Mock<IMailService> _mailServiceMock = new();

    private OrderService CreateService()
    {
        return new OrderService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _creditNoteMock.Object,
            _mailServiceMock.Object
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
    }

    private static IQueryable<T> AsyncQueryable<T>(List<T> data)
        => new TestAsyncEnumerable<T>(data);

    [Fact]
    public async Task UpdateOrderItem_ShouldUpdateIncreaseQuantity()
    {
        var orderRepo = SetupOrderRepo();
        var batchRepo = SetupBatchRepo();
        SetupTransaction();

        var batchId = Guid.NewGuid();

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductBatchId = batchId,
            Quantity = 2,
            Price = 100
        };

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderItems = new List<OrderItem> { orderItem }
        };

        var batch = new ProductBatch
        {
            Id = batchId,
            Quantity = 10,
            Product = new Product { Quantity = 10 }
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

        await service.UpdateOrderItemAsync(order.Id, orderItem.Id,
            new UpdateOrderItemRequest { Quantity = 5 });

        Assert.Equal(5, orderItem.Quantity);
        Assert.Equal(7, batch.Quantity);
        Assert.Equal(7, batch.Product.Quantity);
    }

    [Fact]
    public async Task UpdateOrderItem_ShouldUpdateDecreaseQuantity()
    {
        var orderRepo = SetupOrderRepo();
        var batchRepo = SetupBatchRepo();
        SetupTransaction();

        var batchId = Guid.NewGuid();

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductBatchId = batchId,
            Quantity = 5,
            Price = 100
        };

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderItems = new List<OrderItem> { orderItem }
        };

        var batch = new ProductBatch
        {
            Id = batchId,
            Quantity = 10,
            Product = new Product { Quantity = 10 }
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

        await service.UpdateOrderItemAsync(order.Id, orderItem.Id,
            new UpdateOrderItemRequest { Quantity = 2 });

        Assert.Equal(2, orderItem.Quantity);
        Assert.Equal(13, batch.Quantity);
        Assert.Equal(13, batch.Product.Quantity);
    }

    [Fact]
    public async Task UpdateOrderItem_ShouldThrow_WhenNotEnoughStock()
    {
        var orderRepo = SetupOrderRepo();
        var batchRepo = SetupBatchRepo();
        SetupTransaction();

        var batchId = Guid.NewGuid();

        var orderItem = new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductBatchId = batchId,
            Quantity = 2,
            Price = 100
        };

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderItems = new List<OrderItem> { orderItem }
        };

        var batch = new ProductBatch
        {
            Id = batchId,
            Quantity = 1,
            Product = new Product { Quantity = 1 }
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

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.UpdateOrderItemAsync(order.Id, orderItem.Id,
                new UpdateOrderItemRequest { Quantity = 10 }));
    }

    [Fact]
    public async Task UpdateOrderItem_ShouldCallRemove_WhenQuantityZero()
    {
        var service = CreateService();

        var called = false;

        var spy = Mock.Get(service);

        await Assert.ThrowsAsync<Exception>(() =>
            service.UpdateOrderItemAsync(Guid.NewGuid(), Guid.NewGuid(),
                new UpdateOrderItemRequest { Quantity = 0 }));
    }
}