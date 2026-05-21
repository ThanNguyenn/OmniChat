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
using System.Linq.Expressions;

namespace OmniChat.Test.InvoiceServiceTest;

public class AllocateMoneyToInvoicesTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<InvoiceService>> _loggerMock = new();
    protected readonly Mock<IMailService> _mailServiceMock = new();

    public InvoiceService CreateService()
    {
        return new InvoiceService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _mailServiceMock.Object
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
    public async Task AllocateMoneyToInvoices_ShouldReturn_WhenWalletNotFound()
    {
        var walletRepo = SetupRepository<Wallet>();
        var invoiceRepo = SetupRepository<Invoice>();
        var allocationRepo = SetupRepository<Allocation>();

        SetupTransaction();

        walletRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Wallet, bool>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IOrderedQueryable<Wallet>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IIncludableQueryable<Wallet, object>>>()))
            .ReturnsAsync((Wallet)null);

        var service = CreateService();

        await service.AllocateMoneyToInvoices(Guid.NewGuid());

        allocationRepo.Verify(
            r => r.InsertAsync(It.IsAny<Allocation>()),
            Times.Never);

        invoiceRepo.Verify(
            r => r.UpdateRange(It.IsAny<IEnumerable<Invoice>>()),
            Times.Never);
    }

    [Fact]
    public async Task AllocateMoneyToInvoices_ShouldCompleteInvoice_WhenEnoughMoney()
    {
        var walletRepo = SetupRepository<Wallet>();
        var invoiceRepo = SetupRepository<Invoice>();
        var allocationRepo = SetupRepository<Allocation>();

        SetupTransaction();

        var customerId = Guid.NewGuid();

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Amount = 100
        };

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Total = 100,
            PaidAmount = 0,
            InvoiceStatus = InvoiceStatus.Pending,
            Allocations = new List<Allocation>(),
            StartedDate = DateTime.UtcNow
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
            .ReturnsAsync(new List<Invoice> { invoice });

        var service = CreateService();

        await service.AllocateMoneyToInvoices(customerId);

        Assert.Equal(0, wallet.Amount);
        Assert.Equal(100, invoice.PaidAmount);
        Assert.Equal(InvoiceStatus.Completed, invoice.InvoiceStatus);
        Assert.NotNull(invoice.CompletedDate);

        allocationRepo.Verify(
            r => r.InsertAsync(It.Is<Allocation>(a =>
                a.Amount == 100 &&
                a.InvoiceId == invoice.Id &&
                a.WalletId == wallet.Id)),
            Times.Once);

        walletRepo.Verify(r => r.Update(wallet), Times.Once);

        invoiceRepo.Verify(
            r => r.UpdateRange(It.IsAny<IEnumerable<Invoice>>()),
            Times.Once);
    }

    [Fact]
    public async Task AllocateMoneyToInvoices_ShouldPartiallyPayInvoice_WhenInsufficientMoney()
    {
        var walletRepo = SetupRepository<Wallet>();
        var invoiceRepo = SetupRepository<Invoice>();
        var allocationRepo = SetupRepository<Allocation>();

        SetupTransaction();

        var customerId = Guid.NewGuid();

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Amount = 50
        };

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Total = 100,
            PaidAmount = 0,
            InvoiceStatus = InvoiceStatus.Pending,
            Allocations = new List<Allocation>(),
            StartedDate = DateTime.UtcNow
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
            .ReturnsAsync(new List<Invoice> { invoice });

        var service = CreateService();

        await service.AllocateMoneyToInvoices(customerId);

        Assert.Equal(0, wallet.Amount);
        Assert.Equal(50, invoice.PaidAmount);
        Assert.Equal(InvoiceStatus.Pending, invoice.InvoiceStatus);

        allocationRepo.Verify(
            r => r.InsertAsync(It.Is<Allocation>(a =>
                a.Amount == 50)),
            Times.Once);
    }

    [Fact]
    public async Task AllocateMoneyToInvoices_ShouldAllocateAcrossMultipleInvoices()
    {
        var walletRepo = SetupRepository<Wallet>();
        var invoiceRepo = SetupRepository<Invoice>();
        var allocationRepo = SetupRepository<Allocation>();

        SetupTransaction();

        var customerId = Guid.NewGuid();

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Amount = 150
        };

        var invoice1 = new Invoice
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Total = 100,
            PaidAmount = 0,
            InvoiceStatus = InvoiceStatus.Pending,
            Allocations = new List<Allocation>(),
            StartedDate = DateTime.UtcNow.AddDays(-1)
        };

        var invoice2 = new Invoice
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Total = 100,
            PaidAmount = 0,
            InvoiceStatus = InvoiceStatus.Pending,
            Allocations = new List<Allocation>(),
            StartedDate = DateTime.UtcNow
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
            .ReturnsAsync(new List<Invoice> { invoice1, invoice2 });

        var service = CreateService();

        await service.AllocateMoneyToInvoices(customerId);

        Assert.Equal(0, wallet.Amount);

        Assert.Equal(100, invoice1.PaidAmount);
        Assert.Equal(InvoiceStatus.Completed, invoice1.InvoiceStatus);

        Assert.Equal(50, invoice2.PaidAmount);
        Assert.Equal(InvoiceStatus.Pending, invoice2.InvoiceStatus);

        allocationRepo.Verify(
            r => r.InsertAsync(It.IsAny<Allocation>()),
            Times.Exactly(2));
    }
}