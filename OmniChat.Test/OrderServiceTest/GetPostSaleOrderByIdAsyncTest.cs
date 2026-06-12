using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Order;
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

public class GetPostSaleOrderByIdAsyncTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<OrderService>> _loggerMock = new();
    protected readonly Mock<ICreditNoteService> _creditNoteMock = new();
    protected readonly Mock<IMailService> _mailServiceMock = new();
    private readonly Mock<IProductBatchAuditService> _auditServiceMock = new();
    private readonly Mock<IWalletService> _walletServiceMock = new();
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

    protected Mock<IGenericRepository<Order>> SetupOrderRepo()
    {
        var repo = new Mock<IGenericRepository<Order>>();

        _uowMock.Setup(x => x.GetRepository<Order>())
            .Returns(repo.Object);

        return repo;
    }

    [Fact]
    public async Task GetPostSaleOrderByIdAsync_ShouldReturnOrder_WhenExists()
    {
        var repo = SetupRepository<Order>();
        SetupTransaction();

        var orderId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            OrderItems = new List<OrderItem>()
        };

        var data = new TestAsyncEnumerable<Order>(new List<Order> { order });

        repo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<bool>()))
            .Returns(data);

        _mapperMock
            .Setup(m => m.Map<GetPostSaleOrderResponse>(It.IsAny<Order>()))
            .Returns(new GetPostSaleOrderResponse());

        var service = CreateService();

        var result = await service.GetPostSaleOrderByIdAsync(orderId);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetPostSaleOrderByIdAsync_ShouldThrowNotFound_WhenMissing()
    {
        var repo = SetupRepository<Order>();
        SetupTransaction();

        var data = new TestAsyncEnumerable<Order>(new List<Order>());

        repo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<bool>()))
            .Returns(data);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetPostSaleOrderByIdAsync(Guid.NewGuid()));
    }
}
