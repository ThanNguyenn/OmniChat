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

public class CreateInvoiceTest
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

        _uowMock
    .Setup(x => x.CommitAsync())
    .ReturnsAsync(1);
    }

    [Fact]
    public async Task CreateInvoice_ShouldReturnEmpty_WhenNoOrders()
    {
        var orderRepo = SetupRepository<Order>();
        var invoiceRepo = SetupRepository<Invoice>();
        var walletRepo = SetupRepository<Wallet>();

        SetupTransaction();

        orderRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
            .ReturnsAsync(new List<Order>());

        var service = CreateService();

        var result = await service.CreateInvoice(
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        Assert.Empty(result);

        invoiceRepo.Verify(
            r => r.InsertRangeAsync(It.IsAny<IEnumerable<Invoice>>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateInvoice_ShouldCreateInvoice()
    {
        var orderRepo = SetupRepository<Order>();
        var invoiceRepo = SetupRepository<Invoice>();
        var walletRepo = SetupRepository<Wallet>();

        SetupTransaction();

        var customerId = Guid.NewGuid();

        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                TotalAmount = 1000,
                DeliveryStatus = DeliveryStatus.Completed,
                CreditNotes = new List<CreditNote>()
            }
        };

        orderRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
            .ReturnsAsync(orders);

        invoiceRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>()))
            .ReturnsAsync(new List<Invoice>());

        walletRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Wallet, bool>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IOrderedQueryable<Wallet>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IIncludableQueryable<Wallet, object>>>()))
            .ReturnsAsync(new Wallet
            {
                CustomerId = customerId,
                Amount = 200
            });

        var service = CreateService();

        var result = await service.CreateInvoice(
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        Assert.Single(result);

        invoiceRepo.Verify(r =>
            r.InsertRangeAsync(It.Is<IEnumerable<Invoice>>(i =>
                i.Count() == 1 &&
                i.First().CustomerId == customerId &&
                i.First().Total == 1000 &&
                i.First().DeductedAmount == 200)),
            Times.Once);

        orderRepo.Verify(
            r => r.UpdateRange(It.IsAny<IEnumerable<Order>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateInvoice_ShouldSkip_WhenInvoiceAlreadyExists()
    {
        var orderRepo = SetupRepository<Order>();
        var invoiceRepo = SetupRepository<Invoice>();
        var walletRepo = SetupRepository<Wallet>();

        SetupTransaction();

        var customerId = Guid.NewGuid();

        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                TotalAmount = 1000,
                DeliveryStatus = DeliveryStatus.Completed,
                CreditNotes = new List<CreditNote>()
            }
        };

        orderRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
            .ReturnsAsync(orders);

        invoiceRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>()))
            .ReturnsAsync(new List<Invoice>
            {
                new Invoice
                {
                    CustomerId = customerId
                }
            });

        var service = CreateService();

        var result = await service.CreateInvoice(
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        Assert.Empty(result);

        invoiceRepo.Verify(
            r => r.InsertRangeAsync(It.IsAny<IEnumerable<Invoice>>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateInvoice_ShouldSubtractCompletedCreditNotes()
    {
        var orderRepo = SetupRepository<Order>();
        var invoiceRepo = SetupRepository<Invoice>();
        var walletRepo = SetupRepository<Wallet>();

        SetupTransaction();

        var customerId = Guid.NewGuid();

        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                TotalAmount = 1000,
                DeliveryStatus = DeliveryStatus.Completed,
                CreditNotes = new List<CreditNote>
                {
                    new CreditNote
                    {
                        Total = 300,
                        CreditNoteStatus = CreditNoteStatus.Completed
                    }
                }
            }
        };

        orderRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
            .ReturnsAsync(orders);

        invoiceRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>()))
            .ReturnsAsync(new List<Invoice>());

        walletRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Wallet, bool>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IOrderedQueryable<Wallet>>>(),
                It.IsAny<Func<IQueryable<Wallet>, IIncludableQueryable<Wallet, object>>>()))
            .ReturnsAsync((Wallet)null);

        var service = CreateService();

        await service.CreateInvoice(
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow);

        invoiceRepo.Verify(r =>
            r.InsertRangeAsync(It.Is<IEnumerable<Invoice>>(i =>
                i.First().Total == 700)),
            Times.Once);
    }
}