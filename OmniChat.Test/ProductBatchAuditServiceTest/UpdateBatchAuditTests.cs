using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Requests.ProductBatchAudit;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using Action = OmniChat.Infrastructure.Models.Action;

namespace OmniChat.Test.ProductBatchAuditServiceTest;

public class UpdateBatchAuditTests
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

    private void SetupTransaction()
    {
        _uowMock
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
            .Returns<Func<Task<bool>>>(f => f());
    }

    [Fact]
    public async Task UpdateBatchAudit_ShouldThrow_WhenNotFound()
    {
        var repo = SetupRepo();
        SetupTransaction();

        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((BatchAudit)null);

        var service = CreateService();

        var request = new UpdateBatchAuditRequest
        {
            OldValue = 10,
            NewValue = 20,
            Action = Action.Enter
        };

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateBatchAuditAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task UpdateBatchAudit_ShouldUpdate_AllFields()
    {
        var repo = SetupRepo();
        SetupTransaction();

        var entity = new BatchAudit
        {
            Id = Guid.NewGuid(),
            OldValue = 1,
            NewValue = 2,
            Action = Action.Enter
        };

        repo.Setup(r => r.GetByIdAsync(entity.Id))
            .ReturnsAsync(entity);

        var service = CreateService();

        var request = new UpdateBatchAuditRequest
        {
            OldValue = 100,
            NewValue = 200,
            Action = Action.Export
        };

        _mapperMock.Setup(m => m.Map(request, entity))
            .Callback<UpdateBatchAuditRequest, BatchAudit>((src, dest) =>
            {
                dest.OldValue = src.OldValue ?? dest.OldValue;
                dest.NewValue = src.NewValue ?? dest.NewValue;
                dest.Action = src.Action ?? dest.Action;
            });

        var result = await service.UpdateBatchAuditAsync(entity.Id, request);

        Assert.True(result);
        Assert.Equal(100, entity.OldValue);
        Assert.Equal(200, entity.NewValue);
        Assert.Equal(Action.Export, entity.Action);

        repo.Verify(r => r.Update(entity), Times.Once);
    }

    [Fact]
    public async Task UpdateBatchAudit_ShouldSupport_PartialUpdate()
    {
        var repo = SetupRepo();
        SetupTransaction();

        var entity = new BatchAudit
        {
            Id = Guid.NewGuid(),
            OldValue = 10,
            NewValue = 20,
            Action = Action.Enter
        };

        repo.Setup(r => r.GetByIdAsync(entity.Id))
            .ReturnsAsync(entity);

        var service = CreateService();

        var request = new UpdateBatchAuditRequest
        {
            NewValue = 999 // partial update only
        };

        _mapperMock.Setup(m => m.Map(request, entity))
            .Callback<UpdateBatchAuditRequest, BatchAudit>((src, dest) =>
            {
                if (src.OldValue.HasValue)
                    dest.OldValue = src.OldValue.Value;

                if (src.NewValue.HasValue)
                    dest.NewValue = src.NewValue.Value;

                if (src.Action.HasValue)
                    dest.Action = src.Action.Value;
            });

        var result = await service.UpdateBatchAuditAsync(entity.Id, request);

        Assert.True(result);
        Assert.Equal(10, entity.OldValue);     // unchanged
        Assert.Equal(999, entity.NewValue);    // updated
        Assert.Equal(Action.Enter, entity.Action);

        repo.Verify(r => r.Update(entity), Times.Once);
    }
}