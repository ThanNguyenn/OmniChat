using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
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
    public class UpdateCustomerProfileByIdTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<CustomerProfile>> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IWalletService> _mockWalletService;
        private readonly Mock<IHubContext<SupportConversationHub>> _mockHubContext;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly CustomerProfileService _service;

        public UpdateCustomerProfileByIdTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<CustomerProfile>>();
            _mockMapper = new Mock<IMapper>();
            _mockWalletService = new Mock<IWalletService>();
            _mockHubContext = new Mock<IHubContext<SupportConversationHub>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();

            _mockUow.Setup(u => u.GetRepository<CustomerProfile>()).Returns(_mockRepo.Object);

            
            _mockUow.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task<GetCustomerProfileResponse>>>()))
                    .Returns((Func<Task<GetCustomerProfileResponse>> func) => func());

            _service = new CustomerProfileService(
                _mockUow.Object,
                new Mock<ILogger<CustomerProfileService>>().Object,
                _mockMapper.Object,
                _mockAccessor.Object,
                _mockHubContext.Object,
                _mockWalletService.Object);
        }

        [Fact]
        public async Task UpdateCustomerProfileByIdAsync_ValidRequest_UpdatesAndNotifiesSignalR()
        {
            var customerId = Guid.NewGuid();
            var request = new UpdateCustomerProfileRequest
            {
                CustomerName = "Tên Mới",
                Email = "new@gmail.com"
            };

            var existingCustomer = new CustomerProfile
            {
                Id = customerId,
                CustomerName = "Tên Cũ",
                Email = "old@gmail.com",
                IsNewCustomer = true
            };

            var responseDto = new GetCustomerProfileResponse
            {
                Id = customerId,
                CustomerName = "Tên Mới"
            };

            _mockRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<CustomerProfile, bool>>>(), null, null))
                     .ReturnsAsync(existingCustomer);

            _mockMapper.Setup(m => m.Map<GetCustomerProfileResponse>(existingCustomer))
                       .Returns(responseDto);

 
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);

        
            var result = await _service.UpdateCustomerProfileByIdAsync(customerId, request);

     
            Assert.NotNull(result);
            Assert.Equal("Tên Mới", existingCustomer.CustomerName);
            Assert.False(existingCustomer.IsNewCustomer);

            _mockRepo.Verify(r => r.Update(existingCustomer), Times.Once);

         
            mockClientProxy.Verify(
                p => p.SendCoreAsync(
                    "CustomerProfileUpdated",
                    It.Is<object[]>(o => o[0] == responseDto),
                    default),
                Times.Once);
        }

        [Fact]
        public async Task UpdateCustomerProfileByIdAsync_CustomerNotFound_ThrowsNotFoundException()
        {
            _mockRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<CustomerProfile, bool>>>(), null, null))
                     .ReturnsAsync((CustomerProfile)null);

         
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdateCustomerProfileByIdAsync(Guid.NewGuid(), new UpdateCustomerProfileRequest()));
        }

        [Fact]
        public async Task UpdateCustomerProfileByIdAsync_EmptyCustomerId_ThrowsBadRequestException()
        {
            
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.UpdateCustomerProfileByIdAsync(Guid.Empty, new UpdateCustomerProfileRequest()));
        }
    }
}
