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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Claim = System.Security.Claims.Claim;

namespace OmniChat.Test.PostSaleRequestServiceTest;

public class AcceptPostSaleRequestAsyncTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<PostSaleRequestService>> _loggerMock = new();
    protected readonly Mock<IOrderService> _orderServiceMock = new();

    public PostSaleRequestService CreateService()
    {
        return new PostSaleRequestService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _orderServiceMock.Object
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
    }

    private void SetupHttpContext(Guid userId)
    {
        var claims = new List<Claim>
    {
        new Claim("UserId", userId.ToString())
    };

        var identity = new ClaimsIdentity(claims, "mock");

        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = principal
        };

        _httpMock.Setup(x => x.HttpContext)
            .Returns(context);
    }

    [Fact]
    public async Task AcceptPostSaleRequestAsync_ShouldApproveRefundRequest()
    {
        var requestRepo = SetupRepository<PostSaleRequest>();
        var staffRepo = SetupRepository<Staff>();

        SetupTransaction();

        var requestId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var staffId = Guid.NewGuid();

        SetupHttpContext(accountId);

        var request = new PostSaleRequest
        {
            Id = requestId,
            OrderId = orderId,
            Type = PostSaleRequestType.Refund,
            RefundAmount = 1000,
            Status = PostSaleRequestStatus.Pending
        };

        var staff = new Staff
        {
            Id = staffId,
            AccountId = accountId
        };

        requestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        staffRepo.Setup(r => r.SingleOrDefaultAsync(
        It.IsAny<Expression<Func<Staff, bool>>>(),
        It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
        It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
    .ReturnsAsync(staff);

        _orderServiceMock
            .Setup(x => x.ReturnOrderPaidAsync(orderId, 1000))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.AcceptPostSaleRequestAsync(requestId);

        Assert.True(result);

        Assert.Equal(PostSaleRequestStatus.Approved, request.Status);
        Assert.Equal(staffId, request.ResolveById);

        _orderServiceMock.Verify(
            x => x.ReturnOrderPaidAsync(orderId, 1000),
            Times.Once);

        requestRepo.Verify(
            x => x.Update(It.IsAny<PostSaleRequest>()),
            Times.Once);
    }

    [Fact]
    public async Task AcceptPostSaleRequestAsync_ShouldApproveReturnRequest()
    {
        var requestRepo = SetupRepository<PostSaleRequest>();
        var staffRepo = SetupRepository<Staff>();

        SetupTransaction();

        var requestId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        SetupHttpContext(accountId);

        var request = new PostSaleRequest
        {
            Id = requestId,
            OrderId = orderId,
            Type = PostSaleRequestType.Return,
            RefundAmount = 500
        };

        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            AccountId = accountId
        };

        requestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        staffRepo.Setup(r => r.SingleOrDefaultAsync(
        It.IsAny<Expression<Func<Staff, bool>>>(),
        It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
        It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
    .ReturnsAsync(staff);

        _orderServiceMock
            .Setup(x => x.ReturnOrderUnpaidAsync(orderId, 500))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.AcceptPostSaleRequestAsync(requestId);

        Assert.True(result);

        _orderServiceMock.Verify(
            x => x.ReturnOrderUnpaidAsync(orderId, 500),
            Times.Once);
    }

    [Fact]
    public async Task AcceptPostSaleRequestAsync_ShouldApproveCancelRequest()
    {
        var requestRepo = SetupRepository<PostSaleRequest>();
        var staffRepo = SetupRepository<Staff>();

        SetupTransaction();

        var requestId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        SetupHttpContext(accountId);

        var request = new PostSaleRequest
        {
            Id = requestId,
            OrderId = orderId,
            Type = PostSaleRequestType.Cancel
        };

        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            AccountId = accountId
        };

        requestRepo.Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        staffRepo.Setup(r => r.SingleOrDefaultAsync(
         It.IsAny<Expression<Func<Staff, bool>>>(),
         It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
         It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
     .ReturnsAsync(staff);

        _orderServiceMock
            .Setup(x => x.CancelOrderAsync(orderId))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.AcceptPostSaleRequestAsync(requestId);

        Assert.True(result);

        _orderServiceMock.Verify(
            x => x.CancelOrderAsync(orderId),
            Times.Once);
    }

    [Fact]
    public async Task AcceptPostSaleRequestAsync_ShouldThrow_WhenRequestNotFound()
    {
        var requestRepo = SetupRepository<PostSaleRequest>();

        SetupTransaction();

        requestRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PostSaleRequest?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AcceptPostSaleRequestAsync(Guid.NewGuid()));

        _orderServiceMock.Verify(
            x => x.ReturnOrderPaidAsync(It.IsAny<Guid>(), It.IsAny<double>()),
            Times.Never);
    }
}