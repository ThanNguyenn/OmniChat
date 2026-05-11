using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using Action = OmniChat.Infrastructure.Models.Action;

namespace OmniChat.Test.ProductBatchAuditServiceTest;

public class ProductBatchAuditWrapperTests
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
    public async Task AddAsync_ShouldInsertAudit_WithEnterAction()
    {
        var repo = SetupRepo();
        var service = CreateService();

        var batchId = Guid.NewGuid();

        await service.AddAsync(batchId, 10, 20);

        repo.Verify(r => r.InsertAsync(It.Is<BatchAudit>(a =>
            a.ProductBatchId == batchId &&
            a.OldValue == 10 &&
            a.NewValue == 20 &&
            a.Action == Action.Enter &&
            a.ActionById == null
        )), Times.Once);
    }

    [Fact]
    public async Task ExportAsync_ShouldInsertAudit_WithExportAction()
    {
        var repo = SetupRepo();
        var service = CreateService();

        var batchId = Guid.NewGuid();
        var actionById = Guid.NewGuid();

        await service.ExportAsync(batchId, 5, 15, actionById);

        repo.Verify(r => r.InsertAsync(It.Is<BatchAudit>(a =>
            a.ProductBatchId == batchId &&
            a.OldValue == 5 &&
            a.NewValue == 15 &&
            a.Action == Action.Export &&
            a.ActionById == actionById
        )), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldForceNewValueToZero_AndSetRemoveAction()
    {
        var repo = SetupRepo();
        var service = CreateService();

        var batchId = Guid.NewGuid();
        var actionById = Guid.NewGuid();

        await service.RemoveAsync(batchId, 50, 999, actionById);

        repo.Verify(r => r.InsertAsync(It.Is<BatchAudit>(a =>
            a.ProductBatchId == batchId &&
            a.OldValue == 50 &&
            a.NewValue == 0 &&
            a.Action == Action.Remove &&
            a.ActionById == actionById
        )), Times.Once);
    }
}