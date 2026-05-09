using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Requests.CustomerProfile;
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
    public class EnrichCustomerProfileTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<CustomerProfile>> _mockCustomerRepo;
        private readonly Mock<ICustomerProfileService> _mockProfileService;
        private readonly Mock<IWalletService> _mockWalletService;
        private readonly Mock<IHubContext<SupportConversationHub>> _mockHubContext;
        private readonly CustomerMergeService _service;

        public EnrichCustomerProfileTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockCustomerRepo = new Mock<IGenericRepository<CustomerProfile>>();
            _mockProfileService = new Mock<ICustomerProfileService>();
            _mockWalletService = new Mock<IWalletService>();
            _mockHubContext = new Mock<IHubContext<SupportConversationHub>>();

            _mockUow.Setup(u => u.GetRepository<CustomerProfile>()).Returns(_mockCustomerRepo.Object);

 
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);

            _service = new CustomerMergeService(
                _mockUow.Object,
                new Mock<ILogger<CustomerMergeService>>().Object,
                new Mock<AutoMapper.IMapper>().Object,
                new Mock<IHttpContextAccessor>().Object,
                _mockProfileService.Object,
                new Mock<ICustomerMessageService>().Object,
                new Mock<ISupportConversationService>().Object,
                _mockHubContext.Object,
                new Mock<ISupportStaffMessageService>().Object,
                new Mock<IServiceScopeFactory>().Object,
                _mockWalletService.Object
            );
        }

        [Fact]
        public async Task HandleEnrichCustomerAsync_NewInformation_UpdatesProfileAndCreatesWallet()
        {

            var profileId = Guid.NewGuid();
            var dto = new EnrichCustomerRequest
            {
                ActiveCustomerId = profileId,
                Email = "TEST@GMAIL.COM",
                Phone = "+84 909.123-456",
                Address = "Ho Chi Minh City"
            };

            var currentProfile = new CustomerProfile { Id = profileId, IsProfileCompleted = false };

            _mockProfileService.Setup(s => s.GetCustomerProfileByIdAsync(profileId)).ReturnsAsync(currentProfile);

 
            _mockCustomerRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<CustomerProfile, bool>>>(), null, null))
                             .ReturnsAsync((CustomerProfile)null);


            await _service.HandleEnrichCustomerAsync(dto);

            Assert.Equal("test@gmail.com", currentProfile.Email);
            Assert.Equal("0909123456", currentProfile.PhoneNumber);
            Assert.True(currentProfile.IsProfileCompleted);
            Assert.False(currentProfile.IsNewCustomer);

            _mockWalletService.Verify(s => s.CreateWallet(profileId), Times.Once);
            _mockCustomerRepo.Verify(r => r.Update(currentProfile), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task HandleEnrichCustomerAsync_DuplicateFound_TriggersMergeAndDelete()
        {

            var currentId = Guid.NewGuid();
            var existingId = Guid.NewGuid();
            var dto = new EnrichCustomerRequest
            {
                ActiveCustomerId = currentId,
                Email = "duplicate@gmail.com",
                Phone = "0909000111"
            };

            var currentProfile = new CustomerProfile { Id = currentId };
            var existingProfile = new CustomerProfile { Id = existingId };

            _mockProfileService.Setup(s => s.GetCustomerProfileByIdAsync(currentId)).ReturnsAsync(currentProfile);
            _mockProfileService.Setup(s => s.GetCustomerProfileByIdAsync(existingId)).ReturnsAsync(existingProfile);


            _mockCustomerRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<CustomerProfile, bool>>>(), null, null))
                             .ReturnsAsync(existingProfile);

            await _service.HandleEnrichCustomerAsync(dto);

           
            _mockUow.Verify(u => u.CommitAsync(), Times.Exactly(2));
            _mockCustomerRepo.Verify(r => r.DeleteAsync(It.IsAny<Expression<Func<CustomerProfile, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task HandleEnrichCustomerAsync_ProfileAlreadyCompleted_ReturnsEarly()
        {
           
            var profileId = Guid.NewGuid();
            var currentProfile = new CustomerProfile { Id = profileId, IsProfileCompleted = true };

            _mockProfileService.Setup(s => s.GetCustomerProfileByIdAsync(profileId)).ReturnsAsync(currentProfile);

            
            await _service.HandleEnrichCustomerAsync(new EnrichCustomerRequest { ActiveCustomerId = profileId });

           
            _mockWalletService.Verify(s => s.CreateWallet(It.IsAny<Guid>()), Times.Never);
            _mockCustomerRepo.Verify(r => r.Update(It.IsAny<CustomerProfile>()), Times.Never);
            _mockUow.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task HandleEnrichCustomerAsync_ProfileNotFound_ThrowsNotFoundException()
        {
           
            _mockProfileService.Setup(s => s.GetCustomerProfileByIdAsync(It.IsAny<Guid>()))
                               .ReturnsAsync((CustomerProfile)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.HandleEnrichCustomerAsync(new EnrichCustomerRequest { ActiveCustomerId = Guid.NewGuid() }));
        }

        [Theory]
        [InlineData("+84 909 123 456", "0909123456")]
        [InlineData("0909-123-456", "0909123456")]
        [InlineData("0909.123.456", "0909123456")]
        public async Task NormalizePhone_Logic_WorksCorrectly(string inputPhone, string expectedPhone)
        {
           
            var profileId = Guid.NewGuid();
            var current = new CustomerProfile { Id = profileId, IsProfileCompleted = false };
            var dto = new EnrichCustomerRequest { ActiveCustomerId = profileId, Phone = inputPhone, Email = "test@gmail.com" };

            _mockProfileService.Setup(s => s.GetCustomerProfileByIdAsync(profileId)).ReturnsAsync(current);
            _mockCustomerRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<CustomerProfile, bool>>>(), null, null))
                             .ReturnsAsync((CustomerProfile)null);

            
            await _service.HandleEnrichCustomerAsync(dto);

           
            Assert.Equal(expectedPhone, current.PhoneNumber);
        }
    }
}
