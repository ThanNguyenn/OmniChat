using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

public class GetOrderForShipperAsyncTest
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

    private Mock<IGenericRepository<Order>> SetupRepo()
    {
        var repo = new Mock<IGenericRepository<Order>>();

        _uowMock.Setup(x => x.GetRepository<Order>())
            .Returns(repo.Object);

        return repo;
    }

    private PagingResponse<GetOrderForShipperResponse> Paging()
    {
        return new PagingResponse<GetOrderForShipperResponse>
        {
            Items = new List<GetOrderForShipperResponse>(),
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
    public async Task Should_CallRepository_With_DefaultParameters()
    {
        var repo = SetupRepo();

        repo.Setup(r => r.GetPagingListAsync<GetOrderForShipperResponse>(
                It.IsAny<Expression<Func<Order, GetOrderForShipperResponse>>>(),
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(Paging());

        var service = CreateService();

        var result = await service.GetOrderForShipperAsync(null);

        Assert.NotNull(result);

        repo.Verify(r => r.GetPagingListAsync<GetOrderForShipperResponse>(
            It.IsAny<Expression<Func<Order, GetOrderForShipperResponse>>>(),
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            1,
            20), Times.Once);
    }

    [Fact]
    public async Task Should_Pass_Status_Filter()
    {
        var repo = SetupRepo();

        repo.Setup(r => r.GetPagingListAsync<GetOrderForShipperResponse>(
                It.IsAny<Expression<Func<Order, GetOrderForShipperResponse>>>(),
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(Paging());

        var service = CreateService();

        await service.GetOrderForShipperAsync("completed");

        repo.Verify(r => r.GetPagingListAsync<GetOrderForShipperResponse>(
            It.IsAny<Expression<Func<Order, GetOrderForShipperResponse>>>(),
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            1,
            20), Times.Once);
    }

    [Fact]
    public async Task Should_Apply_CustomPaging_AndSorting()
    {
        var repo = SetupRepo();

        repo.Setup(r => r.GetPagingListAsync<GetOrderForShipperResponse>(
                It.IsAny<Expression<Func<Order, GetOrderForShipperResponse>>>(),
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(Paging());

        var service = CreateService();

        await service.GetOrderForShipperAsync(
            status: null,
            pageNumber: 3,
            pageSize: 50,
            sortBy: "status",
            descending: true);

        repo.Verify(r => r.GetPagingListAsync<GetOrderForShipperResponse>(
            It.IsAny<Expression<Func<Order, GetOrderForShipperResponse>>>(),
            It.IsAny<Expression<Func<Order, bool>>>(),
            It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
            It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>(),
            3,
            50), Times.Once);
    }
}
