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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.OrderServiceTest;

public class ReturnOrderUnpaidAsync
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<OrderService>> _loggerMock = new();
    protected readonly Mock<ICreditNoteService> _creditNoteMock = new();
    protected readonly Mock<IMailService> _mailServiceMock = new();
    protected readonly Mock<IInvoiceService> _invoiceServiceMock = new();
    private readonly Mock<IProductBatchAuditService> _auditServiceMock = new();

    private OrderService CreateService()
    {
        return new OrderService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _creditNoteMock.Object,
            _mailServiceMock.Object,
            _auditServiceMock.Object
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
    public async Task ReturnOrderUnpaidAsync_ShouldReturnTrue_AndCreateCreditNoteAdjustment()
    {
        var orderRepo = SetupRepository<Order>();
        SetupTransaction();

        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            Status = OrderStatus.Completed,
            CustomerProfile = new CustomerProfile
            {
                Id = Guid.NewGuid(),
                CustomerName = "Test",
                Email = "test@test.com",
                Wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customerId,
                    Amount = 0,
                    Transactions = new List<Transaction>()
                }
            },
            Invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Allocations = new List<Allocation>(),
                PaidAmount = 0,
                Total = 1000,
                InvoiceStatus = InvoiceStatus.Pending
            }
        };

        orderRepo
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _creditNoteMock
            .Setup(x => x.CreateCreditNoteAdjustmentAsync(orderId, 1000))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.ReturnOrderUnpaidAsync(orderId, 1000);

        Assert.True(result);

        orderRepo.Verify(r => r.Update(It.Is<Order>(o =>
            o.Id == orderId &&
            o.Status == OrderStatus.Returned
        )), Times.Once);

        _creditNoteMock.Verify(x =>
            x.CreateCreditNoteAdjustmentAsync(orderId, 1000),
            Times.Once);
    }

    [Fact]
    public async Task ReturnOrderUnpaidAsync_ShouldThrow_WhenOrderNotFound()
    {
        var orderRepo = SetupRepository<Order>();
        SetupTransaction();

        orderRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Order?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.ReturnOrderUnpaidAsync(Guid.NewGuid(), 1000));

        _creditNoteMock.Verify(x =>
            x.CreateCreditNoteAdjustmentAsync(It.IsAny<Guid>(), It.IsAny<double>()),
            Times.Never);
    }
}
