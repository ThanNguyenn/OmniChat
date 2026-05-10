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
using System.Security.Claims;
using Claim = System.Security.Claims.Claim;

namespace OmniChat.Test.PostSaleRequestServiceTest;

public class RejectPostSaleRequestAsyncTest
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

        _uowMock
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny < Func < Task >> ()))
            .Returns<Func<Task>>(f => f());
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
    public async Task RejectPostSaleRequestAsync_ShouldRejectRequest()
    {
        var postSaleRepo = SetupRepository<PostSaleRequest>();
        var staffRepo = SetupRepository<Staff>();

        SetupTransaction();

        var requestId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var staffId = Guid.NewGuid();

        SetupHttpContext(accountId);

        var request = new PostSaleRequest
        {
            Id = requestId,
            Status = PostSaleRequestStatus.Pending
        };

        var staff = new Staff
        {
            Id = staffId,
            AccountId = accountId
        };

        postSaleRepo
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        staffRepo
            .Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
                It.IsAny<Func<IQueryable<Staff>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Staff, object>>>()))
            .ReturnsAsync(staff);

        var service = CreateService();

        var result = await service.RejectPostSaleRequestAsync(requestId);

        Assert.True(result);

        postSaleRepo.Verify(r => r.Update(It.Is<PostSaleRequest>(x =>
            x.Status == PostSaleRequestStatus.Rejected &&
            x.ResolveById == staffId &&
            x.ResolvedTime != null
        )), Times.Once);
    }

    [Fact]
    public async Task RejectPostSaleRequestAsync_ShouldThrow_WhenRequestNotFound()
    {
        var postSaleRepo = SetupRepository<PostSaleRequest>();

        SetupTransaction();

        postSaleRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PostSaleRequest?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.RejectPostSaleRequestAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task RejectPostSaleRequestAsync_ShouldThrow_WhenStaffNotFound()
    {
        var postSaleRepo = SetupRepository<PostSaleRequest>();
        var staffRepo = SetupRepository<Staff>();

        SetupTransaction();

        var requestId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        SetupHttpContext(accountId);

        var request = new PostSaleRequest
        {
            Id = requestId,
            Status = PostSaleRequestStatus.Pending
        };

        postSaleRepo
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(request);

        staffRepo
            .Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
                It.IsAny<Func<IQueryable<Staff>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Staff, object>>>()))
            .ReturnsAsync((Staff?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NullReferenceException>(() =>
            service.RejectPostSaleRequestAsync(requestId));
    }
}