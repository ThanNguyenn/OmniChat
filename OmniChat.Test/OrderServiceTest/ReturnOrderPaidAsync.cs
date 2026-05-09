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
using Microsoft.EntityFrameworkCore;
namespace OmniChat.Test.OrderServiceTest;

public class ReturnOrderPaidAsync
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<OrderService>> _loggerMock = new();
    protected readonly Mock<ICreditNoteService> _creditNoteMock = new();
    protected readonly Mock<IMailService> _mailServiceMock = new();
    protected readonly Mock<IInvoiceService> _invoiceServiceMock = new();

    public OrderService CreateService()
    {
        return new OrderService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _creditNoteMock.Object,
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
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
            .Returns<Func<Task<bool>>>(f => f());

        _uowMock
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        _uowMock.SetupGet(x => x.Context)
    .Returns(new OmniChatDbContext(
        new DbContextOptionsBuilder<OmniChatDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options));
    }

    [Fact]
    public async Task ReturnOrderPaidAsync_ShouldReturnTrue_AndCreateCreditNote()
    {
        var orderRepo = SetupRepository<Order>();
        SetupTransaction();

        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Amount = 0,
            Transactions = new List<Transaction>()
        };

        var customer = new CustomerProfile
        {
            Id = Guid.NewGuid(),
            CustomerName = "Test",
            Email = "test@test.com",
            Wallet = wallet
        };

        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.Completed,
            CustomerId = customerId,
            CustomerProfile = customer,
            Invoice = new Invoice()
        };

        orderRepo
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _creditNoteMock
            .Setup(x => x.CreateCreditNoteRefundAsync(It.IsAny<Guid>(), It.IsAny<double>()))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.ReturnOrderPaidAsync(orderId, 1000);

        Assert.True(result);

        orderRepo.Verify(r => r.Update(It.Is<Order>(o =>
            o.Id == orderId &&
            o.Status == OrderStatus.Returned
        )), Times.Once);

        _creditNoteMock.Verify(x =>
            x.CreateCreditNoteRefundAsync(orderId, 1000),
            Times.Once);
    }

    [Fact]
    public async Task ReturnOrderPaidAsync_ShouldThrow_WhenOrderMissing()
    {
        var orderRepo = SetupRepository<Order>();
        SetupTransaction();

        orderRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Order?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ReturnOrderPaidAsync(Guid.NewGuid(), 1000));

        _creditNoteMock.Verify(x =>
            x.CreateCreditNoteRefundAsync(It.IsAny<Guid>(), It.IsAny<double>()),
            Times.Never);
    }
}