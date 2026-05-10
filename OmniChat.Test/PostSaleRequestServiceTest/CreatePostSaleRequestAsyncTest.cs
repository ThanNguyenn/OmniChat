using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.PostSaleRequest;
using OmniChat.Infrastructure.Dtos.Requests.PostSaleRequestItem;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;
using System.Security.Claims;
using Claim = System.Security.Claims.Claim;

namespace OmniChat.Test.PostSaleRequestServiceTest;

public class CreatePostSaleRequestAsyncTest
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
    public async Task CreatePostSaleRequestAsync_ShouldReturnTrue_WhenValid()
    {
        var postSaleRepo = SetupRepository<PostSaleRequest>();
        var staffRepo = SetupRepository<Staff>();
        var orderItemRepo = SetupRepository<OrderItem>();

        SetupTransaction();

        var accountId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        SetupHttpContext(accountId);

        staffRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Staff, bool>>>(),
                null,
                null))
            .ReturnsAsync(new Staff
            {
                Id = staffId,
                AccountId = accountId
            });

        orderItemRepo.Setup(r => r.GetListAsync(
        It.IsAny<Expression<Func<OrderItem, bool>>>(),
        It.IsAny<Func<IQueryable<OrderItem>, IOrderedQueryable<OrderItem>>>(),
        It.IsAny<Func<IQueryable<OrderItem>, IIncludableQueryable<OrderItem, object>>>()))
    .ReturnsAsync(new List<OrderItem>
    {
        new OrderItem
        {
            Id = orderItemId,
            Quantity = 10,
            Price = 100
        }
    });

        postSaleRepo.Setup(r => r.InsertAsync(It.IsAny<PostSaleRequest>()))
            .Returns(Task.CompletedTask);

        var request = new CreatePostSaleRequestRequest
        {
            CustomerId = customerId,
            OrderId = orderId,
            Type = PostSaleRequestType.Refund,
            Reason = "Broken item",
            PostSaleItems = new List<CreatePostSaleRequestItemRequest>
            {
                new CreatePostSaleRequestItemRequest
                {
                    OrderItemId = orderItemId,
                    Quantity = 2
                }
            }
        };

        var service = CreateService();

        var result = await service.CreatePostSaleRequestAsync(request);

        Assert.True(result);

        postSaleRepo.Verify(r => r.InsertAsync(
            It.Is<PostSaleRequest>(x =>
                x.CustomerId == customerId &&
                x.OrderId == orderId &&
                x.PresentByStaffId == staffId &&
                x.RefundAmount == 200 &&
                x.Status == PostSaleRequestStatus.Pending
            )),
            Times.Once);
    }

    [Fact]
    public async Task CreatePostSaleRequestAsync_ShouldThrow_WhenStaffNotFound()
    {
        var staffRepo = SetupRepository<Staff>();
        var orderItemRepo = SetupRepository<OrderItem>();

        SetupTransaction();

        var accountId = Guid.NewGuid();

        SetupHttpContext(accountId);

        staffRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Staff, bool>>>(),
                null,
                null))
            .ReturnsAsync((Staff?)null);

        var request = new CreatePostSaleRequestRequest
        {
            CustomerId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Type = PostSaleRequestType.Refund,
            Reason = "Broken item",
            PostSaleItems = new List<CreatePostSaleRequestItemRequest>
            {
                new CreatePostSaleRequestItemRequest
                {
                    OrderItemId = Guid.NewGuid(),
                    Quantity = 1
                }
            }
        };

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreatePostSaleRequestAsync(request));
    }

    [Fact]
    public async Task CreatePostSaleRequestAsync_ShouldThrow_WhenQuantityInvalid()
    {
        var postSaleRepo = SetupRepository<PostSaleRequest>();
        var staffRepo = SetupRepository<Staff>();
        var orderItemRepo = SetupRepository<OrderItem>();

        SetupTransaction();

        var accountId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        SetupHttpContext(accountId);

        staffRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Staff, bool>>>(),
                null,
                null))
            .ReturnsAsync(new Staff
            {
                Id = Guid.NewGuid(),
                AccountId = accountId
            });

        orderItemRepo.Setup(r => r.GetListAsync(
            It.IsAny<Expression<Func<OrderItem, bool>>>(),
            It.IsAny<Func<IQueryable<OrderItem>, IOrderedQueryable<OrderItem>>>(),
            It.IsAny<Func<IQueryable<OrderItem>, IIncludableQueryable<OrderItem, object>>>()))
        .ReturnsAsync(new List<OrderItem>
        {
        new OrderItem
        {
            Id = orderItemId,
            Quantity = 1,
            Price = 100
        }
        });
        var request = new CreatePostSaleRequestRequest
        {
            CustomerId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Type = PostSaleRequestType.Refund,
            Reason = "Broken item",
            PostSaleItems = new List<CreatePostSaleRequestItemRequest>
            {
                new CreatePostSaleRequestItemRequest
                {
                    OrderItemId = orderItemId,
                    Quantity = 5
                }
            }
        };

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreatePostSaleRequestAsync(request));

        postSaleRepo.Verify(r =>
            r.InsertAsync(It.IsAny<PostSaleRequest>()),
            Times.Never);
    }
}