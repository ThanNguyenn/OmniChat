using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatchAudit;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.ProductBatchAuditServiceTest;

public class GetDetailByBatchIdTest
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

    [Fact]
    public async Task GetDetail_ShouldReturnMappedResult_WhenEntityExists()
    {
        var repo = SetupRepo();
        var service = CreateService();

        var id = Guid.NewGuid();

        var entity = new BatchAudit
        {
            Id = id
        };

        repo.Setup(r => r.GetQueryable(
         It.IsAny<Expression<Func<BatchAudit, bool>>>(),
         It.IsAny<Func<IQueryable<BatchAudit>, IQueryable<BatchAudit>>>(),
         It.IsAny<bool>()))
     .Returns(new TestAsyncEnumerable<BatchAudit>(new List<BatchAudit> { entity }));

        _mapperMock.Setup(m => m.Map<GetDetailByBatchIdResponse>(entity))
            .Returns(new GetDetailByBatchIdResponse());

        var result = await service.GetDetailByBatchIdAsync(id);

        Assert.NotNull(result);

        _mapperMock.Verify(m => m.Map<GetDetailByBatchIdResponse>(entity), Times.Once);
    }

    [Fact]
    public async Task GetDetail_ShouldCallMapper_WithNull_WhenNotFound()
    {
        var repo = SetupRepo();
        var service = CreateService();

        var id = Guid.NewGuid();

        repo.Setup(r => r.GetQueryable(
          It.IsAny<Expression<Func<BatchAudit, bool>>>(),
          It.IsAny<Func<IQueryable<BatchAudit>, IQueryable<BatchAudit>>>(),
          It.IsAny<bool>()))
      .Returns(new TestAsyncEnumerable<BatchAudit>(
          new List<BatchAudit>().AsQueryable()
      ));

        _mapperMock.Setup(m => m.Map<GetDetailByBatchIdResponse>(null))
            .Returns(new GetDetailByBatchIdResponse());

        var result = await service.GetDetailByBatchIdAsync(id);

        _mapperMock.Verify(m => m.Map<GetDetailByBatchIdResponse>(null), Times.Once);
    }
}