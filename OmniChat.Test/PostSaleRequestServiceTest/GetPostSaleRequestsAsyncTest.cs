using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.PostSaleRequest;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.PostSaleRequestServiceTest;

public class GetPostSaleRequestsAsyncTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<PostSaleRequestService>> _loggerMock = new();
    protected readonly Mock<IOrderService> _orderServiceMock = new();

    public PostSaleRequestService CreateService()
    {
        return new PostSaleRequestService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _orderServiceMock.Object
        );
    }

    protected Mock<IGenericRepository<T>> SetupRepository<T>() where T : class
    {
        var repoMock = new Mock<IGenericRepository<T>>();

        _uowMock.Setup(x => x.GetRepository<T>())
            .Returns(repoMock.Object);

        return repoMock;
    }

    private PagingResponse<GetPostSaleRequestsResponse> CreatePagingResponse()
    {
        return new PagingResponse<GetPostSaleRequestsResponse>
        {
            Items = new List<GetPostSaleRequestsResponse>
            {
                new GetPostSaleRequestsResponse()
            },
            Meta = new PaginationMeta
            {
                CurrentPage = 1,
                PageSize = 20,
                TotalItems = 1,
                TotalPages = 1
            }
        };
    }

    [Fact]
    public async Task GetPostSaleRequestsAsync_ShouldReturnPagingResponse()
    {
        var repo = SetupRepository<PostSaleRequest>();

        var expected = CreatePagingResponse();

        repo.Setup(r => r.GetPagingListAsync<GetPostSaleRequestsResponse>(
                It.IsAny<Expression<Func<PostSaleRequest, GetPostSaleRequestsResponse>>>(),
                It.IsAny<Expression<Func<PostSaleRequest, bool>>>(),
                It.IsAny<Func<IQueryable<PostSaleRequest>, IOrderedQueryable<PostSaleRequest>>>(),
                It.IsAny<Func<IQueryable<PostSaleRequest>, IIncludableQueryable<PostSaleRequest, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(expected);

        var service = CreateService();

        var result = await service.GetPostSaleRequestsAsync();

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.Meta.TotalItems);
        Assert.Equal(1, result.Meta.TotalPages);
        Assert.Equal(1, result.Meta.CurrentPage);
        Assert.Equal(20, result.Meta.PageSize);

        repo.Verify(r => r.GetPagingListAsync<GetPostSaleRequestsResponse>(
            It.IsAny<Expression<Func<PostSaleRequest, GetPostSaleRequestsResponse>>>(),
            It.IsAny<Expression<Func<PostSaleRequest, bool>>>(),
            It.IsAny<Func<IQueryable<PostSaleRequest>, IOrderedQueryable<PostSaleRequest>>>(),
            It.IsAny<Func<IQueryable<PostSaleRequest>, IIncludableQueryable<PostSaleRequest, object>>>(),
            1,
            20), Times.Once);
    }

    [Fact]
    public async Task GetPostSaleRequestsAsync_ShouldPassCustomPaging()
    {
        var repo = SetupRepository<PostSaleRequest>();

        var expected = new PagingResponse<GetPostSaleRequestsResponse>
        {
            Items = new List<GetPostSaleRequestsResponse>(),
            Meta = new PaginationMeta
            {
                CurrentPage = 2,
                PageSize = 5,
                TotalItems = 0,
                TotalPages = 0
            }
        };

        repo.Setup(r => r.GetPagingListAsync<GetPostSaleRequestsResponse>(
                It.IsAny<Expression<Func<PostSaleRequest, GetPostSaleRequestsResponse>>>(),
                It.IsAny<Expression<Func<PostSaleRequest, bool>>>(),
                It.IsAny<Func<IQueryable<PostSaleRequest>, IOrderedQueryable<PostSaleRequest>>>(),
                It.IsAny<Func<IQueryable<PostSaleRequest>, IIncludableQueryable<PostSaleRequest, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(expected);

        var service = CreateService();

        var result = await service.GetPostSaleRequestsAsync(
            pageNumber: 2,
            pageSize: 5);

        Assert.Equal(2, result.Meta.CurrentPage);
        Assert.Equal(5, result.Meta.PageSize);

        repo.Verify(r => r.GetPagingListAsync<GetPostSaleRequestsResponse>(
            It.IsAny<Expression<Func<PostSaleRequest, GetPostSaleRequestsResponse>>>(),
            It.IsAny<Expression<Func<PostSaleRequest, bool>>>(),
            It.IsAny<Func<IQueryable<PostSaleRequest>, IOrderedQueryable<PostSaleRequest>>>(),
            It.IsAny<Func<IQueryable<PostSaleRequest>, IIncludableQueryable<PostSaleRequest, object>>>(),
            2,
            5), Times.Once);
    }

    [Fact]
    public async Task GetPostSaleRequestsAsync_ShouldPassSorting()
    {
        var repo = SetupRepository<PostSaleRequest>();

        repo.Setup(r => r.GetPagingListAsync<GetPostSaleRequestsResponse>(
                It.IsAny<Expression<Func<PostSaleRequest, GetPostSaleRequestsResponse>>>(),
                It.IsAny<Expression<Func<PostSaleRequest, bool>>>(),
                It.IsAny<Func<IQueryable<PostSaleRequest>, IOrderedQueryable<PostSaleRequest>>>(),
                It.IsAny<Func<IQueryable<PostSaleRequest>, IIncludableQueryable<PostSaleRequest, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(CreatePagingResponse());

        var service = CreateService();

        var result = await service.GetPostSaleRequestsAsync(
            sortBy: "status",
            descending: false);

        Assert.NotNull(result);

        repo.Verify(r => r.GetPagingListAsync<GetPostSaleRequestsResponse>(
            It.IsAny<Expression<Func<PostSaleRequest, GetPostSaleRequestsResponse>>>(),
            It.IsAny<Expression<Func<PostSaleRequest, bool>>>(),
            It.IsAny<Func<IQueryable<PostSaleRequest>, IOrderedQueryable<PostSaleRequest>>>(),
            It.IsAny<Func<IQueryable<PostSaleRequest>, IIncludableQueryable<PostSaleRequest, object>>>(),
            1,
            20), Times.Once);
    }
}