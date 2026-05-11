using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.WalletServiceTest;

public class CreateWalletTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<WalletService>> _loggerMock = new();
    protected readonly Mock<IInvoiceService> _invoiceServiceMock = new();

    public WalletService CreateService()
    {
        return new WalletService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _invoiceServiceMock.Object
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

        _uowMock
            .Setup(x => x.CommitAsync())
            .ReturnsAsync(1);
    }

    [Fact]
    public async Task CreateWallet_ShouldThrow_WhenWalletAlreadyExists()
    {
        var walletRepo = SetupRepository<Wallet>();

        var customerId = Guid.NewGuid();

        walletRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Wallet, bool>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IOrderedQueryable<Wallet>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IIncludableQueryable<Wallet, object>>>()))
            .ReturnsAsync(new Wallet { CustomerId = customerId });

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateWallet(customerId));

        walletRepo.Verify(r =>
            r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Wallet, bool>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IOrderedQueryable<Wallet>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IIncludableQueryable<Wallet, object>>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateWallet_ShouldCreateWallet_WhenNotExists()
    {
        var walletRepo = SetupRepository<Wallet>();

        var customerId = Guid.NewGuid();

        walletRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Wallet, bool>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IOrderedQueryable<Wallet>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IIncludableQueryable<Wallet, object>>>()))
            .ReturnsAsync((Wallet)null);

        SetupTransaction();

        var service = CreateService();

        var result = await service.CreateWallet(customerId);

        Assert.True(result);

        walletRepo.Verify(r =>
            r.InsertAsync(It.Is<Wallet>(w =>
                w.CustomerId == customerId)),
            Times.Once);
    }
}