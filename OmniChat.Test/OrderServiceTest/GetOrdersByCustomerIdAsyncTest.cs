using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Order;
using OmniChat.Infrastructure.Metadatas;
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

public class GetOrdersByCustomerIdAsyncTest
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

    private PagingResponse<GetOrderResponse> CreatePagingResult()
    {
        return new PagingResponse<GetOrderResponse>
        {
            Items = new List<GetOrderResponse>(),
            Meta = new PaginationMeta
            {
                CurrentPage = 1,
                PageSize = 20,
                TotalItems = 0,
                TotalPages = 0
            }
        };
    }

    [Fact]
    public async Task GetOrdersByCustomerIdAsync_ShouldReturnData()
    {
        var repo = SetupOrderRepo();
        SetupTransaction();

        var customerId = Guid.NewGuid();

        var expected = CreatePagingResult();

        repo.Setup(r => r.GetPagingListAsync<GetOrderResponse>(
                It.IsAny<Expression<Func<Order, GetOrderResponse>>>(),
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(expected);

        var service = CreateService();

        var result = await service.GetOrdersByCustomerIdAsync(
            customerId,
            null,
            null,
            1,
            20);

        Assert.NotNull(result);

        repo.Verify(r => r.GetPagingListAsync<GetOrderResponse>(
            It.IsAny<Expression<Func<Order, GetOrderResponse>>>(),
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            1,
            20), Times.Once);
    }

    [Fact]
    public async Task GetOrdersByCustomerIdAsync_ShouldApplyStatusFilter()
    {
        var repo = SetupOrderRepo();
        SetupTransaction();

        var customerId = Guid.NewGuid();

        repo.Setup(r => r.GetPagingListAsync<GetOrderResponse>(
                It.IsAny<Expression<Func<Order, GetOrderResponse>>>(),
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(CreatePagingResult());

        var service = CreateService();

        var statuses = new[] { OrderStatus.Pending };

        await service.GetOrdersByCustomerIdAsync(
            customerId,
            statuses,
            null);

        repo.Verify(r => r.GetPagingListAsync<GetOrderResponse>(
            It.IsAny<Expression<Func<Order, GetOrderResponse>>>(),
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            1,
            20), Times.Once);
    }

    [Fact]
    public async Task GetOrdersByCustomerIdAsync_ShouldApplySearch()
    {
        var repo = SetupOrderRepo();
        SetupTransaction();

        var customerId = Guid.NewGuid();

        repo.Setup(r => r.GetPagingListAsync<GetOrderResponse>(
                It.IsAny<Expression<Func<Order, GetOrderResponse>>>(),
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(CreatePagingResult());

        var service = CreateService();

        await service.GetOrdersByCustomerIdAsync(
            customerId,
            null,
            "ABC");

        repo.Verify(r => r.GetPagingListAsync<GetOrderResponse>(
            It.IsAny<Expression<Func<Order, GetOrderResponse>>>(),
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            1,
            20), Times.Once);
    }
}
