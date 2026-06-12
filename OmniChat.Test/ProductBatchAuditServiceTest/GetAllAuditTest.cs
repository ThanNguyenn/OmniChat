using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatchAudit;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;
using Action = OmniChat.Infrastructure.Models.Action;

namespace OmniChat.Test.ProductBatchAuditServiceTest;

public class GetAllAuditTest
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    private readonly Mock<ILogger<ProductBatchAuditService>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IHttpContextAccessor> _httpMock = new();

    private ProductBatchAuditService CreateService()
    {
        return new ProductBatchAuditService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object
        );
    }

    private Mock<IGenericRepository<BatchAudit>> SetupRepo()
    {
        var repo = new Mock<IGenericRepository<BatchAudit>>();

        _uowMock.Setup(x => x.GetRepository<BatchAudit>())
            .Returns(repo.Object);

        return repo;
    }

    private static List<BatchAudit> GetData()
    {
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var batchId1 = Guid.NewGuid();
        var batchId2 = Guid.NewGuid();

        return new List<BatchAudit>
        {
            new BatchAudit
            {
                Id = Guid.NewGuid(),
                ProductBatchId = batchId1,
                ProductBatch = new ProductBatch
                {
                    Id = batchId1,
                    ProductId = productId1
                },
                Action = Action.Enter
            },
            new BatchAudit
            {
                Id = Guid.NewGuid(),
                ProductBatchId = batchId2,
                ProductBatch = new ProductBatch
                {
                    Id = batchId2,
                    ProductId = productId2
                },
                Action = Action.Export
            }
        };
    }

    [Fact]
    public async Task GetAllAudit_ShouldFilter_ByProductId()
    {
        var repo = SetupRepo();
        var service = CreateService();

        var data = GetData().AsQueryable();


        var productId = data.First().ProductBatch.ProductId;

        Expression<Func<BatchAudit, bool>> capturedPredicate = null;

        repo.Setup(r => r.GetPagingListAsync<GetAllAuditResponse>(
         It.IsAny<Expression<Func<BatchAudit, GetAllAuditResponse>>>(),
         It.IsAny<Expression<Func<BatchAudit, bool>>>(),
         It.IsAny<Func<IQueryable<BatchAudit>, IOrderedQueryable<BatchAudit>>>(),
         It.IsAny<Func<IQueryable<BatchAudit>, IIncludableQueryable<BatchAudit, object>>>(),
         It.IsAny<int>(),
         It.IsAny<int>()))
     .Callback<
         Expression<Func<BatchAudit, GetAllAuditResponse>>,
         Expression<Func<BatchAudit, bool>>,
         Func<IQueryable<BatchAudit>, IOrderedQueryable<BatchAudit>>,
         Func<IQueryable<BatchAudit>, IIncludableQueryable<BatchAudit, object>>,
         int,
         int>((sel, pred, ord, inc, p, s) =>
         {
             capturedPredicate = pred;
         })
     .ReturnsAsync(new PagingResponse<GetAllAuditResponse>());

        await service.GetAllAuditAsync(null,productId, null, null);

        var compiled = capturedPredicate!.Compile();

        Assert.True(data.All(x => compiled(x) == (x.ProductBatch.ProductId == productId)));
    }

    [Fact]
    public async Task GetAllAudit_ShouldFilter_ByBatchId()
    {
        var repo = SetupRepo();
        var service = CreateService();

        var data = GetData().AsQueryable();

        var batchId = data.First().ProductBatchId;

        Expression<Func<BatchAudit, bool>> capturedPredicate = null;

        repo.Setup(r => r.GetPagingListAsync<GetAllAuditResponse>(
         It.IsAny<Expression<Func<BatchAudit, GetAllAuditResponse>>>(),
         It.IsAny<Expression<Func<BatchAudit, bool>>>(),
         It.IsAny<Func<IQueryable<BatchAudit>, IOrderedQueryable<BatchAudit>>>(),
         It.IsAny<Func<IQueryable<BatchAudit>, IIncludableQueryable<BatchAudit, object>>>(),
         It.IsAny<int>(),
         It.IsAny<int>()))
     .Callback<
         Expression<Func<BatchAudit, GetAllAuditResponse>>,
         Expression<Func<BatchAudit, bool>>,
         Func<IQueryable<BatchAudit>, IOrderedQueryable<BatchAudit>>,
         Func<IQueryable<BatchAudit>, IIncludableQueryable<BatchAudit, object>>,
         int,
         int>((sel, pred, ord, inc, p, s) =>
         {
             capturedPredicate = pred;
         })
     .ReturnsAsync(new PagingResponse<GetAllAuditResponse>());

        await service.GetAllAuditAsync( null, batchId,null, null);

        var compiled = capturedPredicate!.Compile();

        Assert.True(data.All(x => compiled(x) == (x.ProductBatchId == batchId)));
    }

    [Fact]
    public async Task GetAllAudit_ShouldFilter_ByAction()
    {
        var repo = SetupRepo();
        var service = CreateService();

        var data = GetData().AsQueryable();

        var action = Action.Export;

        Expression<Func<BatchAudit, bool>> capturedPredicate = null;

        repo.Setup(r => r.GetPagingListAsync<GetAllAuditResponse>(
        It.IsAny<Expression<Func<BatchAudit, GetAllAuditResponse>>>(),
        It.IsAny<Expression<Func<BatchAudit, bool>>>(),
        It.IsAny<Func<IQueryable<BatchAudit>, IOrderedQueryable<BatchAudit>>>(),
        It.IsAny<Func<IQueryable<BatchAudit>, IIncludableQueryable<BatchAudit, object>>>(),
        It.IsAny<int>(),
        It.IsAny<int>()))
    .Callback<
        Expression<Func<BatchAudit, GetAllAuditResponse>>,
        Expression<Func<BatchAudit, bool>>,
        Func<IQueryable<BatchAudit>, IOrderedQueryable<BatchAudit>>,
        Func<IQueryable<BatchAudit>, IIncludableQueryable<BatchAudit, object>>,
        int,
        int>((sel, pred, ord, inc, p, s) =>
        {
            capturedPredicate = pred;
        })
    .ReturnsAsync(new PagingResponse<GetAllAuditResponse>());

        await service.GetAllAuditAsync(null, null, action,null);

        var compiled = capturedPredicate!.Compile();

        Assert.True(data.All(x => compiled(x) == (x.Action == action)));
    }
}