using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.CreditNoteServiceTest;

public class CreateCreditNoteRefundTest
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    private readonly Mock<ILogger<CreditNoteService>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IHttpContextAccessor> _httpMock = new();
    private readonly Mock<IInvoiceService> _invoiceServiceMock = new();

    private CreditNoteService CreateService()
    {
        return new CreditNoteService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _invoiceServiceMock.Object
        );
    }

    private Mock<IGenericRepository<T>> SetupRepository<T>() where T : class
    {
        var repo = new Mock<IGenericRepository<T>>();

        _uowMock.Setup(x => x.GetRepository<T>())
            .Returns(repo.Object);

        return repo;
    }

    private void SetupTransaction()
    {
        _uowMock
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());
    }

    [Fact]
    public async Task CreateCreditNoteRefund_ShouldProcessSuccessfully()
    {
        var creditNoteRepo = SetupRepository<CreditNote>();
        var orderRepo = SetupRepository<Order>();

        SetupTransaction();

        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            Amount = 100,
            Transactions = new List<Transaction>()
        };

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            CustomerProfile = new CustomerProfile
            {
                Wallet = wallet
            },
            Invoice = new Invoice
            {
                InvoiceStatus = InvoiceStatus.Completed
            }
        };

        var data = new List<Order> { order }.AsQueryable();

        orderRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<bool>()))
            .Returns(new TestAsyncEnumerable<Order>(data));

        var service = CreateService();

        var result = await service.CreateCreditNoteRefundAsync(orderId, 50);

        Assert.True(result);

        creditNoteRepo.Verify(r => r.InsertAsync(It.Is<CreditNote>(c =>
            c.OrderId == orderId &&
            c.Total == 50 &&
            c.CreditNoteType == CreditNoteType.Refund &&
            c.CreditNoteStatus == CreditNoteStatus.Completed)),
            Times.Once);

        Assert.Equal(150, wallet.Amount);
        Assert.Single(wallet.Transactions);

        _invoiceServiceMock.Verify(x =>
            x.AllocateMoneyToInvoices(customerId),
            Times.Once);
    }

    [Fact]
    public async Task CreateCreditNoteRefund_ShouldThrow_WhenAmountInvalid()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateCreditNoteRefundAsync(Guid.NewGuid(), 0));
    }
}