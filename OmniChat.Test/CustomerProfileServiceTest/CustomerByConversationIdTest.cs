using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Responses.Wallet;
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

namespace OmniChat.Test.CustomerProfileServiceTest
{
    public class CustomerByConversationIdTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<SupportConversation>> _mockConvRepo;
        private readonly Mock<IGenericRepository<CustomerProfile>> _mockCustRepo;
        private readonly Mock<IGenericRepository<Provider>> _mockProvRepo;
        private readonly Mock<IWalletService> _mockWalletService;
        private readonly Mock<IHubContext<SupportConversationHub>> _mockHubContext;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly CustomerProfileService _service;

        public CustomerByConversationIdTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockConvRepo = new Mock<IGenericRepository<SupportConversation>>();
            _mockCustRepo = new Mock<IGenericRepository<CustomerProfile>>();
            _mockProvRepo = new Mock<IGenericRepository<Provider>>();
            _mockWalletService = new Mock<IWalletService>();
            _mockHubContext = new Mock<IHubContext<SupportConversationHub>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();

            _mockUow.Setup(u => u.GetRepository<SupportConversation>()).Returns(_mockConvRepo.Object);
            _mockUow.Setup(u => u.GetRepository<CustomerProfile>()).Returns(_mockCustRepo.Object);
            _mockUow.Setup(u => u.GetRepository<Provider>()).Returns(_mockProvRepo.Object);

            _service = new CustomerProfileService(
                _mockUow.Object,
                new Mock<ILogger<CustomerProfileService>>().Object,
                new Mock<AutoMapper.IMapper>().Object,
                _mockAccessor.Object,
                _mockHubContext.Object,
                _mockWalletService.Object);
        }

        [Fact]
        public async Task GetCustomerDetailByConversationIdAsync_ValidId_ReturnsFullDetail()
        {
            var convId = Guid.NewGuid();
            var custId = Guid.NewGuid();
            var provId = Guid.NewGuid();

            var conversation = new SupportConversation
            {
                Id = convId,
                ActiveCustomerId = custId,
                ProvidersId = provId,
                CreatedDate = DateTime.UtcNow
            };

            var customer = new CustomerProfile
            {
                Id = custId,
                CustomerName = "Cường Đô La",
                Orders = new List<Order> { new Order() },
                Invoices = new List<Invoice> { new Invoice { Total = 200, DeductedAmount = 50 } }
            };

            var provider = new Provider { Id = provId, ProviderName = "Facebook Messenger" };

            _mockConvRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<SupportConversation, bool>>>(), null, It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()))
                .ReturnsAsync(conversation);

            _mockCustRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<CustomerProfile, bool>>>(), null, It.IsAny<Func<IQueryable<CustomerProfile>, IIncludableQueryable<CustomerProfile, object>>>()))
                .ReturnsAsync(customer);

            _mockProvRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<Provider, bool>>>(), null, null))
                .ReturnsAsync(provider);

            _mockWalletService.Setup(s => s.CalculateWallet(custId))
                .ReturnsAsync(new GetWalletResponse { Amount = 500 });

            // Act
            var result = await _service.GetCustomerDetailByConversationIdAsync(convId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Cường Đô La", result.CustomerName);
            Assert.Equal("Facebook Messenger", result.ProviderName);
            Assert.Equal(150, result.TotalPay); // 200 - 50
            Assert.Equal(500, result.getWalletResponse.Amount);
        }

        [Fact]
        public async Task GetCustomerDetailByConversationIdAsync_EmptyId_ThrowsBadRequest()
        {
            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.GetCustomerDetailByConversationIdAsync(Guid.Empty));
        }

        [Fact]
        public async Task GetCustomerDetailByConversationIdAsync_ConversationNotFound_ThrowsNotFound()
        {
            // Arrange
            _mockConvRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<SupportConversation, bool>>>(), null, It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()))
                .ReturnsAsync((SupportConversation)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetCustomerDetailByConversationIdAsync(Guid.NewGuid()));
            Assert.Equal("Cuộc hội thoại hỗ trợ không tồn tại.", ex.Message);
        }

        [Fact]
        public async Task GetCustomerDetailByConversationIdAsync_CustomerNotFound_ThrowsNotFound()
        {
            // Arrange
            var conversation = new SupportConversation { Id = Guid.NewGuid(), ActiveCustomerId = Guid.NewGuid() };

            _mockConvRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<SupportConversation, bool>>>(), null, It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()))
                .ReturnsAsync(conversation);

            _mockCustRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<CustomerProfile, bool>>>(), null, It.IsAny<Func<IQueryable<CustomerProfile>, IIncludableQueryable<CustomerProfile, object>>>()))
                .ReturnsAsync((CustomerProfile)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetCustomerDetailByConversationIdAsync(conversation.Id));
            Assert.Equal("Không tìm thấy thông tin khách hàng liên quan đến hội thoại này.", ex.Message);
        }
    }
}
