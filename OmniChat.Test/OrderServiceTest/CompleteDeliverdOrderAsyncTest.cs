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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.OrderServiceTest;

public class CompleteDeliverdOrderAsyncTest
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uow = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ILogger<OrderService>> _logger = new();
    private readonly Mock<IHttpContextAccessor> _http = new();
    private readonly Mock<ICreditNoteService> _credit = new();
    private readonly Mock<IMailService> _mail = new();
    private readonly Mock<IProductBatchAuditService> _auditServiceMock = new(); private readonly Mock<IWalletService> _walletServiceMock = new();
    private OrderService CreateService()
        => new OrderService(
            _uow.Object,
            _logger.Object,
            _mapper.Object,
            _http.Object,
            _credit.Object,
            _mail.Object,
            _auditServiceMock .Object,
            _walletServiceMock.Object
        );

    private Mock<IGenericRepository<Order>> SetupRepo()
    {
        var repo = new Mock<IGenericRepository<Order>>();
        _uow.Setup(x => x.GetRepository<Order>()).Returns(repo.Object);
        return repo;
    }

    private void SetupTx()
    {
        _uow.Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
            .Returns<Func<Task<bool>>>(f => f());
    }

    [Fact]
    public async Task Should_CompleteOrder_AndSendEmail()
    {
        var repo = SetupRepo();
        SetupTx();

        var orderId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            Code = "ORD-1",
            CustomerProfile = new CustomerProfile
            {
                Email = "test@mail.com",
                CustomerName = "John"
            }
        };

        repo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
            .ReturnsAsync(order);

        var service = CreateService();

        var result = await service.CompleteDeliverdOrderAsync(orderId);

        Assert.True(result);

        repo.Verify(r => r.Update(It.Is<Order>(o =>
            o.Status == OrderStatus.Shipped &&
            o.DeliveryStatus == DeliveryStatus.Completed
        )), Times.Once);

        _mail.Verify(m => m.SendEmailAsync(It.IsAny<Infrastructure.Dtos.Requests.Mail.MailContent>()), Times.Once);
    }

    [Fact]
    public async Task Should_Throw_WhenOrderNotFound()
    {
        var repo = SetupRepo();
        SetupTx();

        repo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
            .ReturnsAsync((Order?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CompleteDeliverdOrderAsync(Guid.NewGuid()));
    }
}
