using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Threading.Tasks;
using Xunit;

namespace OmniChat.Test.OrderServiceTest;

public class SubmitOrderAsyncTest
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    private readonly Mock<ILogger<OrderService>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IHttpContextAccessor> _httpMock = new();
    private readonly Mock<ICreditNoteService> _creditNoteMock = new();
    private readonly Mock<IMailService> _mailServiceMock = new();

    private OrderService CreateService()
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

    private Mock<IGenericRepository<Order>> SetupRepo()
    {
        var repo = new Mock<IGenericRepository<Order>>();

        _uowMock.Setup(x => x.GetRepository<Order>())
            .Returns(repo.Object);

        return repo;
    }

    private void SetupTransaction()
    {
        _uowMock.Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
            .Returns<Func<Task<bool>>>(f => f());
    }

    [Fact]
    public async Task SubmitOrder_ShouldReturnTrue_WhenDraftOrder()
    {
        var repo = SetupRepo();
        SetupTransaction();

        var orderId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.Draft
        };

        repo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        var service = CreateService();

        var result = await service.SubmitOrderAsync(orderId);

        Assert.True(result);

        repo.Verify(r => r.Update(It.Is<Order>(o =>
            o.Id == orderId &&
            o.Status == OrderStatus.Pending
        )), Times.Once);
    }

    [Fact]
    public async Task SubmitOrder_ShouldThrow_NotFound()
    {
        var repo = SetupRepo();
        SetupTransaction();

        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Order)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.SubmitOrderAsync(Guid.NewGuid()));

        repo.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task SubmitOrder_ShouldThrow_WhenNotDraft()
    {
        var repo = SetupRepo();
        SetupTransaction();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Status = OrderStatus.Completed
        };

        repo.Setup(r => r.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.SubmitOrderAsync(order.Id));

        repo.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
    }
}