using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Order;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.DraftOrderServiceTest;

public class CreateDraftOrderFromConversationAsyncTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<DraftOrderService>> _loggerMock = new();
    protected readonly Mock<IOrderService> _orderService = new();

    public DraftOrderService CreateService()
    {
        return new DraftOrderService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _orderService.Object
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
    public async Task CreateDraftOrderFromConversationAsync_ShouldReturnTrue_WhenConfirmed()
    {
        var conversationRepo = SetupRepository<SupportConversation>();
        SetupTransaction();

        var conversationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var brandId = Guid.NewGuid();

        var brandRepo = new Mock<IGenericRepository<Brand>>();
        brandRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Brand, bool>>>(),
                It.IsAny<Func<IQueryable<Brand>, IQueryable<Brand>>>(),
                It.IsAny<bool>()))
            .Returns(new TestAsyncEnumerable<Brand>(new List<Brand>
            {
            new Brand { Id = brandId, Name = "long thanh" },
            new Brand { Id = Guid.NewGuid(), Name = "lothamilk" }
            }));

        _uowMock.Setup(x => x.GetRepository<Brand>())
            .Returns(brandRepo.Object);

        var batchRepo = new Mock<IGenericRepository<ProductBatch>>();
        batchRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<ProductBatch, bool>>>(),
                It.IsAny<Func<IQueryable<ProductBatch>, IQueryable<ProductBatch>>>(),
                It.IsAny<bool>()))
            .Returns(new TestAsyncEnumerable<ProductBatch>(new List<ProductBatch>
            {
            new ProductBatch
            {
                Id = Guid.NewGuid(),
                Quantity = 100,
                IsActive = true,
                ManuFactureDate = DateTime.UtcNow,
                Product = new Product
                {
                    VolumeMl = 490,
                    ProductKind = ProductKind.NoSugar,
                    BrandId = brandId, 
                    Price = 1000,
                    Quantity = 100
                }
            }
            }));

        _uowMock.Setup(x => x.GetRepository<ProductBatch>())
            .Returns(batchRepo.Object);

        var messages = new List<CustomerMessage>
    {
        new CustomerMessage
        {
            Content = "cho em 20 chai 490ml ko đường",
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds()
        },
        new CustomerMessage
        {
            Content = "ok dong y",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        }
    };

        var conversation = new SupportConversation
        {
            Id = conversationId,
            ActiveCustomerId = customerId,
            CustomerMessages = messages
        };

        var queryable =
            new TestAsyncEnumerable<SupportConversation>(
                new List<SupportConversation> { conversation });

        conversationRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IQueryable<SupportConversation>>>(),
                It.IsAny<bool>()))
            .Returns(queryable);

        _orderService
            .Setup(x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()))
            .ReturnsAsync(true);

        var service = CreateService();

        var result =
            await service.CreateDraftOrderFromConversationAsync(conversationId);

        Assert.True(result);

        _orderService.Verify(
            x => x.CreateOrderAsync(It.IsAny<CreateOrderRequest>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateDraftOrderFromConversationAsync_ShouldThrow_WhenConversationNotFound()
    {
        var conversationRepo = SetupRepository<SupportConversation>();
        SetupTransaction();

        var conversationId = Guid.NewGuid();

        var queryable =
            new TestAsyncEnumerable<SupportConversation>(
                new List<SupportConversation>());

        conversationRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IQueryable<SupportConversation>>>(),
                It.IsAny<bool>()))
            .Returns(queryable);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.CreateDraftOrderFromConversationAsync(conversationId));
    }

    [Fact]
    public async Task CreateDraftOrderFromConversationAsync_ShouldThrow_WhenNoConfirmation()
    {
        var conversationRepo = SetupRepository<SupportConversation>();
        SetupTransaction();

        var conversationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var messages = new List<CustomerMessage>
        {
            new CustomerMessage
            {
                Content = "hello",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            }
        };

        var conversation = new SupportConversation
        {
            Id = conversationId,
            ActiveCustomerId = customerId,
            CustomerMessages = messages
        };

        var queryable =
            new TestAsyncEnumerable<SupportConversation>(
                new List<SupportConversation> { conversation });

        conversationRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IQueryable<SupportConversation>>>(),
                It.IsAny<bool>()))
            .Returns(queryable);

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateDraftOrderFromConversationAsync(conversationId));
    }
}