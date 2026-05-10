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
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.OrderServiceTest;

public class DeleteOrderAsyncTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<OrderService>> _loggerMock = new();
    protected readonly Mock<ICreditNoteService> _creditNoteMock = new();
    protected readonly Mock<IMailService> _mailServiceMock = new();
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
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());
    }

    [Fact]
    public async Task DeleteOrderAsync_ShouldMarkDeleted_AndCallUpdate()
    {
        var repo = SetupRepository<Order>();
        SetupTransaction();

        var orderId = Guid.NewGuid();

        var order = new Order
        {
            Id = orderId,
            IsDeleted = false
        };

        repo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        var service = CreateService();

        var result = await service.DeleteOrderAsync(orderId);

        Assert.True(result);
        Assert.True(order.IsDeleted);

        repo.Verify(r => r.Update(It.Is<Order>(o =>
            o == order &&
            o.IsDeleted == true
        )), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderAsync_ShouldThrowNotFound_WhenOrderMissing()
    {
        var repo = SetupRepository<Order>();
        SetupTransaction();

        var orderId = Guid.NewGuid();

        repo.Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.DeleteOrderAsync(orderId));
    }
}
