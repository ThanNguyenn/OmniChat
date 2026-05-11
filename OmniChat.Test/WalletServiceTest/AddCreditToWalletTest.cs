using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

public class AddCreditToWalletTest
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
    private OmniChatDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OmniChatDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new OmniChatDbContext(options);
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
    public async Task AddCreditToWallet_ShouldThrow_WhenWalletNotFound()
    {
        var walletRepo = SetupRepository<Wallet>();

        var customerId = Guid.NewGuid();

        walletRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Wallet, bool>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IOrderedQueryable<Wallet>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IIncludableQueryable<Wallet, object>>>()))
            .ReturnsAsync((Wallet)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AddCreditToWallet(customerId, 100));

        walletRepo.Verify(r =>
            r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Wallet, bool>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IOrderedQueryable<Wallet>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IIncludableQueryable<Wallet, object>>>()),
            Times.Once);
    }

    [Fact]
    public async Task AddCreditToWallet_ShouldIncreaseWalletAndCreateTransaction()
    {
        var dbContext = CreateDbContext();

        var customerId = Guid.NewGuid();
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Amount = 200,
            Transactions = new List<Transaction>()
        };

        dbContext.Wallets.Add(wallet);
        await dbContext.SaveChangesAsync();

        var uowMock = new Mock<IUnitOfWork<OmniChatDbContext>>();
        var walletRepoMock = new Mock<IGenericRepository<Wallet>>();

        uowMock.Setup(x => x.Context).Returns(dbContext);

        uowMock.Setup(x => x.GetRepository<Wallet>())
            .Returns(walletRepoMock.Object);

        walletRepoMock.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Wallet, bool>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IOrderedQueryable<Wallet>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IIncludableQueryable<Wallet, object>>>()))
            .ReturnsAsync(wallet);

        uowMock.Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = new WalletService(
            uowMock.Object,
            Mock.Of<ILogger<WalletService>>(),
            Mock.Of<IMapper>(),
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IInvoiceService>());

        var result = await service.AddCreditToWallet(customerId, 150);

        Assert.True(result);
        Assert.Equal(350, wallet.Amount);
        Assert.Single(wallet.Transactions);
    }
}