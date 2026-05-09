using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.ProductServiceTest;

public class UpdateBatchExpiryAsyncTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<ProductService>> _loggerMock = new();
    protected readonly Mock<IR2StorageService> _storageMock = new();

    protected ProductService CreateService()
    {
        return new ProductService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _storageMock.Object
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
    public async Task UpdateBatchExpiryAsync_ShouldMarkExpiredBatchesInactive()
    {
        var repo = SetupRepository<ProductBatch>();

        SetupTransaction();

        var batches = new List<ProductBatch>
    {
        new ProductBatch
        {
            Id = Guid.NewGuid(),
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
            IsExpired = false,
            IsActive = true
        },
        new ProductBatch
        {
            Id = Guid.NewGuid(),
            ExpiryDate = DateTime.UtcNow.AddDays(-2),
            IsExpired = false,
            IsActive = true
        }
    };

        repo.Setup(r => r.GetListAsync(
            It.IsAny<Expression<Func<ProductBatch, bool>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IOrderedQueryable<ProductBatch>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IIncludableQueryable<ProductBatch, object>>>()))
        .ReturnsAsync(batches);

        var service = CreateService();

        await service.UpdateBatchExpiryAsync();

        Assert.All(batches, batch =>
        {
            Assert.True(batch.IsExpired);
            Assert.False(batch.IsActive);
        });

        repo.Verify(r =>
            r.Update(It.IsAny<ProductBatch>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateBatchExpiryAsync_ShouldNotCallUpdate_WhenNoExpiredBatches()
    {
        var repo = SetupRepository<ProductBatch>();

        SetupTransaction();

        repo.Setup(r => r.GetListAsync(
            It.IsAny<Expression<Func<ProductBatch, bool>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IOrderedQueryable<ProductBatch>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IIncludableQueryable<ProductBatch, object>>>()))
        .ReturnsAsync(new List<ProductBatch>());

        var service = CreateService();

        await service.UpdateBatchExpiryAsync();

        repo.Verify(r =>
            r.Update(It.IsAny<ProductBatch>()),
            Times.Never);
    }
}
