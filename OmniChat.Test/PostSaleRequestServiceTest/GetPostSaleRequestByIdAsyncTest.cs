using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.OrderItem;
using OmniChat.Infrastructure.Dtos.Responses.PostSaleRequest;
using OmniChat.Infrastructure.Dtos.Responses.PostSaleRequestItem;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatch;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.PostSaleRequestServiceTest;

public class GetPostSaleRequestByIdAsyncTest
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

    [Fact]
    public async Task GetPostSaleRequestByIdAsync_ShouldReturnResponse()
    {
        var repo = SetupRepository<PostSaleRequest>();

        var id = Guid.NewGuid();

        var entity = new PostSaleRequest
        {
            Id = id
        };

        repo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<PostSaleRequest, bool>>>(),
                It.IsAny<Func<IQueryable<PostSaleRequest>, IOrderedQueryable<PostSaleRequest>>>(),
                It.IsAny<Func<IQueryable<PostSaleRequest>, IIncludableQueryable<PostSaleRequest, object>>>()))
            .ReturnsAsync(entity);

        _mapperMock
            .Setup(m => m.Map<GetPostSaleRequestByIdResponse>(entity))
            .Returns(new GetPostSaleRequestByIdResponse());

        var service = CreateService();

        var result = await service.GetPostSaleRequestByIdAsync(id);

        Assert.NotNull(result);

        _mapperMock.Verify(
            m => m.Map<GetPostSaleRequestByIdResponse>(entity),
            Times.Once);
    }

    [Fact]
    public async Task GetPostSaleRequestByIdAsync_ShouldThrow_WhenNotFound()
    {
        var repo = SetupRepository<PostSaleRequest>();

        repo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<PostSaleRequest, bool>>>(),
                It.IsAny<Func<IQueryable<PostSaleRequest>, IOrderedQueryable<PostSaleRequest>>>(),
                It.IsAny<Func<IQueryable<PostSaleRequest>, IIncludableQueryable<PostSaleRequest, object>>>()))
            .ReturnsAsync((PostSaleRequest)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetPostSaleRequestByIdAsync(Guid.NewGuid()));
    }
}