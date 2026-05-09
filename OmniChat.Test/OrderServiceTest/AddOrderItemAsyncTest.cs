using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
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

public class AddOrderItemAsyncTest
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
    public async Task AddOrderItem_ShouldCreateNewItem_WhenNotExists()
    {
        var orderRepo = SetupOrderRepo();
        var batchRepo = SetupBatchRepo();
        SetupTransaction();

        var orderId = Guid.NewGuid();
        var batchId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            OrderItems = new List<OrderItem>()
        };

        var batch = new ProductBatch
        {
            Id = batchId,
            Quantity = 10,
            Product = new Product { Price = 100 }
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

        var result = await service.AddOrderItemAsync(orderId, new AddOrderItemRequest
        {
            ProductBatchId = batchId,
            Quantity = 2
        });

        Assert.True(result);
        Assert.Single(order.OrderItems);
        Assert.Equal(8, batch.Quantity);
    }

    [Fact]
    public async Task AddOrderItem_ShouldIncreaseQuantity_WhenItemExists()
    {
        var orderRepo = SetupOrderRepo();
        var batchRepo = SetupBatchRepo();
        SetupTransaction();

        var batchId = Guid.NewGuid();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    ProductBatchId = batchId,
                    Quantity = 1,
                    Price = 100
                }
            }
        };

        var batch = new ProductBatch
        {
            Id = batchId,
            Quantity = 10,
            Product = new Product { Price = 100 }
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

        await service.AddOrderItemAsync(order.Id, new AddOrderItemRequest
        {
            ProductBatchId = batchId,
            Quantity = 3
        });

        Assert.Equal(4, order.OrderItems.First().Quantity);
        Assert.Equal(7, batch.Quantity);
    }

    [Fact]
    public async Task AddOrderItem_ShouldThrow_WhenOrderNotFound()
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
            service.AddOrderItemAsync(Guid.NewGuid(), new AddOrderItemRequest
            {
                ProductBatchId = Guid.NewGuid(),
                Quantity = 1
            }));
    }
}