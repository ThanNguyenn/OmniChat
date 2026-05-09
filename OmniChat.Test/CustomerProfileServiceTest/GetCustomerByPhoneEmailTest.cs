using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
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
    public class GetCustomerByPhoneEmailTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<CustomerProfile>> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IWalletService> _mockWalletService;
        private readonly Mock<IHubContext<SupportConversationHub>> _mockHubContext;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly CustomerProfileService _service;

        public GetCustomerByPhoneEmailTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<CustomerProfile>>();
            _mockMapper = new Mock<IMapper>();
            _mockWalletService = new Mock<IWalletService>();
            _mockHubContext = new Mock<IHubContext<SupportConversationHub>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();

            _mockUow.Setup(u => u.GetRepository<CustomerProfile>()).Returns(_mockRepo.Object);

            _service = new CustomerProfileService(
                _mockUow.Object,
                new Mock<ILogger<CustomerProfileService>>().Object,
                _mockMapper.Object,
                _mockAccessor.Object,
                _mockHubContext.Object,
                _mockWalletService.Object);
        }

        [Fact]
        public async Task GetCustomerProfileByEmailOrPhoneAsync_ValidKeyword_ReturnsProfileWithWallet()
        {

            var keyword = "test@gmail.com";
            var customerId = Guid.NewGuid();
            var customerEntity = new CustomerProfile
            {
                Id = customerId,
                Email = keyword,
                PhoneNumber = "0909123456"
            };

            var expectedResponse = new GetCustomerProfileResponse
            {
                Id = customerId,
                Email = keyword
            };


            _mockRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<CustomerProfile, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<CustomerProfile>, IIncludableQueryable<CustomerProfile, object>>>()
            )).ReturnsAsync(customerEntity);

 
            _mockMapper.Setup(m => m.Map<GetCustomerProfileResponse>(customerEntity))
                       .Returns(expectedResponse);

            _mockWalletService.Setup(s => s.CalculateWallet(customerId))
                             .ReturnsAsync(new GetWalletResponse { Amount = 100 });


            var result = await _service.GetCustomerProfileByEmailOrPhoneAsync(keyword);

            Assert.NotNull(result);
            Assert.Equal(keyword, result.Email);
            Assert.Equal(100, result.getWalletResponse.Amount);

            _mockRepo.Verify(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<CustomerProfile, bool>>>(), null, It.IsAny<Func<IQueryable<CustomerProfile>, IIncludableQueryable<CustomerProfile, object>>>()), Times.Once);
            _mockWalletService.Verify(s => s.CalculateWallet(customerId), Times.Once);
        }

        [Fact]
        public async Task GetCustomerProfileByEmailOrPhoneAsync_KeywordEmpty_ThrowsBadRequest()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.GetCustomerProfileByEmailOrPhoneAsync(" "));

            Assert.Equal("Vui lòng cung cấp Email hoặc Số điện thoại để tìm kiếm.", ex.Message);
        }

        [Fact]
        public async Task GetCustomerProfileByEmailOrPhoneAsync_NotFound_ThrowsNotFound()
        {
            // Arrange
            _mockRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<CustomerProfile, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<CustomerProfile>, IIncludableQueryable<CustomerProfile, object>>>()
            )).ReturnsAsync((CustomerProfile)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetCustomerProfileByEmailOrPhoneAsync("unknown@gmail.com"));

            Assert.Equal("Không tìm thấy khách hàng với thông tin đã cung cấp.", ex.Message);
        }

        [Fact]
        public async Task GetCustomerProfileByEmailOrPhoneAsync_MapResultNull_DoesNotCallWallet()
        {

            var customerEntity = new CustomerProfile { Id = Guid.NewGuid() };

            _mockRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<CustomerProfile, bool>>>(), null, It.IsAny<Func<IQueryable<CustomerProfile>, IIncludableQueryable<CustomerProfile, object>>>()))
                .ReturnsAsync(customerEntity);

       
            _mockMapper.Setup(m => m.Map<GetCustomerProfileResponse>(customerEntity))
                       .Returns((GetCustomerProfileResponse)null);

            var result = await _service.GetCustomerProfileByEmailOrPhoneAsync("test@gmail.com");

            Assert.Null(result);
            _mockWalletService.Verify(s => s.CalculateWallet(It.IsAny<Guid>()), Times.Never);
        }
    }
}
