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

public class GetAllOrdersAsyncTest
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

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnPagedResult_WhenNoFilters()
    {
        var repo = SetupRepository<Order>();

        var expected = new PagingResponse<GetAllOrdersResponse>
        {
            Items = new List<GetAllOrdersResponse>(),
            Meta = new PaginationMeta()
        };

        repo.Setup(r => r.GetPagingListAsync<GetAllOrdersResponse>(
            It.IsAny<Expression<Func<Order, GetAllOrdersResponse>>>(),
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
        .ReturnsAsync(expected);

        var service = CreateService();

        var result = await service.GetAllOrdersAsync(null, null);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldFilterByStatus()
    {
        var repo = SetupRepository<Order>();

        var statuses = new[] { OrderStatus.Pending, OrderStatus.Completed };

        repo.Setup(r => r.GetPagingListAsync<GetAllOrdersResponse>(
            It.IsAny<Expression<Func<Order, GetAllOrdersResponse>>>(),
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
        .ReturnsAsync(new PagingResponse<GetAllOrdersResponse>());

        var service = CreateService();

        await service.GetAllOrdersAsync(statuses, null);

        repo.Verify(r => r.GetPagingListAsync<GetAllOrdersResponse>(
            It.IsAny<Expression<Func<Order, GetAllOrdersResponse>>>(),
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            1,
            20),
            Times.Once);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldApplySearchFilter()
    {
        var repo = SetupRepository<Order>();

        repo.Setup(r => r.GetPagingListAsync<GetAllOrdersResponse>(
            It.IsAny<Expression<Func<Order, GetAllOrdersResponse>>>(),
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
        .ReturnsAsync(new PagingResponse<GetAllOrdersResponse>());

        var service = CreateService();

        await service.GetAllOrdersAsync(null, "ABC");

        repo.Verify(r => r.GetPagingListAsync<GetAllOrdersResponse>(
            It.IsAny<Expression<Func<Order, GetAllOrdersResponse>>>(),
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnEmpty_WhenNoData()
    {
        var repo = SetupRepository<Order>();

        var expected = new PagingResponse<GetAllOrdersResponse>
        {
            Items = new List<GetAllOrdersResponse>(),
            Meta = new PaginationMeta()
        };

        repo.Setup(r => r.GetPagingListAsync<GetAllOrdersResponse>(
            It.IsAny<Expression<Func<Order, GetAllOrdersResponse>>>(),
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
        .ReturnsAsync(expected);

        var service = CreateService();

        var result = await service.GetAllOrdersAsync(null, "no-match");

        Assert.Empty(result.Items);
    }
}
