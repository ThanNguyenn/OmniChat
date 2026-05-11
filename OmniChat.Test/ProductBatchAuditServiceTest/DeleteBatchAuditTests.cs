using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using Action = OmniChat.Infrastructure.Models.Action;

namespace OmniChat.Test.ProductBatchAuditServiceTest;

public class DeleteBatchAuditTests
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    private readonly Mock<ILogger<ProductBatchAuditService>> _loggerMock = new();
    private readonly Mock<AutoMapper.IMapper> _mapperMock = new();
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
    public async Task DeleteBatchAudit_ShouldThrow_WhenNotFound()
    {
        var repo = SetupRepo();
        SetupTransaction();

        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((BatchAudit)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.DeleteBatchAuditAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteBatchAudit_ShouldDelete_WhenExists()
    {
        var repo = SetupRepo();
        SetupTransaction();

        var audit = new BatchAudit
        {
            Id = Guid.NewGuid(),
            OldValue = 10,
            NewValue = 20,
            Action = Action.Enter
        };

        repo.Setup(r => r.GetByIdAsync(audit.Id))
            .ReturnsAsync(audit);

        var service = CreateService();

        var result = await service.DeleteBatchAuditAsync(audit.Id);

        Assert.True(result);

        repo.Verify(r => r.Delete(audit), Times.Once);
    }
}