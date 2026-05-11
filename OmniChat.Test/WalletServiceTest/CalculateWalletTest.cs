using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Wallet;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.WalletServiceTest;

public class CalculateWalletTest
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

    [Fact]
    public async Task CalculateWallet_ShouldReturnWalletWithTotalDebt_WhenWalletExists()
    {
        var walletRepo = SetupRepository<Wallet>();
        var invoiceRepo = SetupRepository<Invoice>();

        var customerId = Guid.NewGuid();

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Transactions = new List<Transaction>()
        };

        var invoices = new List<Invoice>
    {
        new Invoice
        {
            CustomerId = customerId,
            Total = 1300,
            DeductedAmount = 200,
            PaidAmount = 100,
            InvoiceStatus = InvoiceStatus.Pending,
            IsDeleted = false
        },
        new Invoice
        {
            CustomerId = customerId,
            Total = 500,
            DeductedAmount = 0,
            PaidAmount = 300,
            InvoiceStatus = InvoiceStatus.PartialPaid,
            IsDeleted = false
        }
    };

        walletRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Wallet, bool>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IOrderedQueryable<Wallet>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IIncludableQueryable<Wallet, object>>>()))
            .ReturnsAsync(wallet);

        invoiceRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>()))
            .ReturnsAsync(invoices);

        _mapperMock
            .Setup(m => m.Map<GetWalletResponse>(It.IsAny<Wallet>()))
            .Returns(new GetWalletResponse());

        var service = CreateService();

        var result = await service.CalculateWallet(customerId);

        Assert.NotNull(result);
        Assert.Equal(1200, result.TotalDebt);
    }

    [Fact]
    public async Task CalculateWallet_ShouldReturnZeroDebt_WhenNoInvoices()
    {
        var walletRepo = SetupRepository<Wallet>();
        var invoiceRepo = SetupRepository<Invoice>();

        var customerId = Guid.NewGuid();

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Transactions = new List<Transaction>()
        };

        walletRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Wallet, bool>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IOrderedQueryable<Wallet>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IIncludableQueryable<Wallet, object>>>()))
            .ReturnsAsync(wallet);

        invoiceRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>()))
            .ReturnsAsync(new List<Invoice>());

        _mapperMock
            .Setup(m => m.Map<GetWalletResponse>(It.IsAny<Wallet>()))
            .Returns(new GetWalletResponse());

        var service = CreateService();

        var result = await service.CalculateWallet(customerId);

        Assert.NotNull(result);
        Assert.Equal(0, result.TotalDebt);
    }

    [Fact]
    public async Task CalculateWallet_ShouldReturnZeroDebt_WhenWalletIsNull()
    {
        var walletRepo = SetupRepository<Wallet>();
        var invoiceRepo = SetupRepository<Invoice>();

        var customerId = Guid.NewGuid();

        walletRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Wallet, bool>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IOrderedQueryable<Wallet>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IIncludableQueryable<Wallet, object>>>()))
            .ReturnsAsync((Wallet)null);

        invoiceRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>()))
            .ReturnsAsync(new List<Invoice>());

        _mapperMock
            .Setup(m => m.Map<GetWalletResponse>(It.IsAny<Wallet>()))
            .Returns(new GetWalletResponse());

        var service = CreateService();

        var result = await service.CalculateWallet(customerId);

        Assert.NotNull(result);
        Assert.Equal(0, result.TotalDebt);
    }
}