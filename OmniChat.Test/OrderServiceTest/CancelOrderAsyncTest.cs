using AutoMapper;
using Microsoft.AspNetCore.Http;
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
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.OrderServiceTest;

public class CancelOrderAsyncTest
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
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
            .Returns<Func<Task<bool>>>(f => f());

        _uowMock
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());
    }

    protected Mock<IGenericRepository<Order>> SetupOrderRepo()
    {
        var repo = new Mock<IGenericRepository<Order>>();

        _uowMock.Setup(x => x.GetRepository<Order>())
            .Returns(repo.Object);

        return repo;
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldThrowNotFound_WhenOrderMissing()
    {
        var orderRepo = SetupRepository<Order>();
        var batchRepo = SetupRepository<ProductBatch>();

        SetupTransaction();

        orderRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
            .ReturnsAsync((Order?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CancelOrderAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task CancelOrderAsync_ShouldThrowBusinessException_WhenNotPending()
    {
        var orderRepo = SetupRepository<Order>();
        var batchRepo = SetupRepository<ProductBatch>();

        SetupTransaction();

        var orderId = Guid.NewGuid();

        orderRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
            .ReturnsAsync(new Order
            {
                Id = orderId,
                Status = OrderStatus.Completed,
                OrderItems = new List<OrderItem>()
            });

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CancelOrderAsync(orderId));
    }
    [Fact]
    public async Task CancelOrderAsync_ShouldCancelOrder_WhenValid()
    {
        var orderRepo = SetupRepository<Order>();
        var batchRepo = SetupRepository<ProductBatch>();

        SetupTransaction();

        var orderId = Guid.NewGuid();
        var batchId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.Pending,
            OrderItems = new List<OrderItem>
        {
            new OrderItem
            {
                Quantity = 2,
                ProductBatchId = batchId
            }
        }
        };

        orderRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
            .ReturnsAsync(order);

        batchRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<ProductBatch, bool>>>(),
                It.IsAny<Func<IQueryable<ProductBatch>, IOrderedQueryable<ProductBatch>>>(),
                It.IsAny<Func<IQueryable<ProductBatch>, IIncludableQueryable<ProductBatch, object>>>()))
            .ReturnsAsync(new List<ProductBatch>
            {
            new ProductBatch
            {
                Id = batchId,
                Quantity = 10,
                Product = new Product
                {
                    Quantity = 100
                }
            }
            });

        var service = CreateService();

        var result = await service.CancelOrderAsync(orderId);

        Assert.True(result);

        orderRepo.Verify(r => r.Update(It.Is<Order>(o =>
            o.Id == orderId &&
            o.Status == OrderStatus.Cancelled
        )), Times.Once);
    }
}
