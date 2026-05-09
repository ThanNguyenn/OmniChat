using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
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
    public class MergeAndDeleteProfileTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<CustomerProfile>> _mockCustomerRepo;
        private readonly Mock<ICustomerProfileService> _mockProfileService;
        private readonly Mock<ICustomerMessageService> _mockMessageService;
        private readonly Mock<ISupportConversationService> _mockConvService;
        private readonly Mock<IHubContext<SupportConversationHub>> _mockHubContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly CustomerMergeService _service;

        public MergeAndDeleteProfileTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockCustomerRepo = new Mock<IGenericRepository<CustomerProfile>>();
            _mockProfileService = new Mock<ICustomerProfileService>();
            _mockMessageService = new Mock<ICustomerMessageService>();
            _mockConvService = new Mock<ISupportConversationService>();
            _mockHubContext = new Mock<IHubContext<SupportConversationHub>>();
            _mockMapper = new Mock<IMapper>();

            _mockUow.Setup(u => u.GetRepository<CustomerProfile>()).Returns(_mockCustomerRepo.Object);


            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);

            _service = new CustomerMergeService(
                _mockUow.Object,
                new Mock<ILogger<CustomerMergeService>>().Object,
                _mockMapper.Object,
                new Mock<IHttpContextAccessor>().Object,
                _mockProfileService.Object,
                _mockMessageService.Object,
                _mockConvService.Object,
                _mockHubContext.Object,
                new Mock<ISupportStaffMessageService>().Object,
                new Mock<IServiceScopeFactory>().Object,
                new Mock<IWalletService>().Object
            );
        }

        [Fact]
        public async Task MergeAndDeleteAsync_ValidIds_MergesSuccessfully()
        {

            var sourceId = Guid.NewGuid();
            var targetId = Guid.NewGuid();

            var source = new CustomerProfile
            {
                Id = sourceId,
                CustomerName = "Source User",
                FacebookSenderId = "FB_123",
                Email = "source@test.com"
            };

            var target = new CustomerProfile
            {
                Id = targetId,
                CustomerName = "Target User",
                Email = null 
            };

            _mockProfileService.Setup(s => s.GetCustomerProfileByIdAsync(sourceId)).ReturnsAsync(source);
            _mockProfileService.Setup(s => s.GetCustomerProfileByIdAsync(targetId)).ReturnsAsync(target);

            _mockMapper.Setup(m => m.Map<GetCustomerProfileResponse>(target))
                       .Returns(new GetCustomerProfileResponse { Id = targetId });

  
            var result = await _service.MergeAndDeleteAsync(sourceId, targetId);


            Assert.NotNull(result);
            Assert.Equal("source@test.com", target.Email); 
            Assert.Equal("Target User", target.CustomerName); 
            Assert.Equal("FB_123", target.FacebookSenderId);


            _mockMessageService.Verify(s => s.UpdateCustomerMessageAfterMergeAsync(source, target), Times.Once);
            _mockConvService.Verify(s => s.UpdateConversationAfterMergeAsync(source, target), Times.Once);

 
            _mockCustomerRepo.Verify(r => r.Update(target), Times.Once);
            _mockCustomerRepo.Verify(r => r.DeleteAsync(It.IsAny<Expression<Func<CustomerProfile, bool>>>()), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Exactly(2)); 
        }

        [Fact]
        public async Task MergeAndDeleteAsync_SameId_ThrowsBadRequest()
        {

            var id = Guid.NewGuid();
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.MergeAndDeleteAsync(id, id));
        }

        [Fact]
        public async Task MergeAndDeleteAsync_NotFound_ThrowsNotFoundException()
        {
           
            _mockProfileService.Setup(s => s.GetCustomerProfileByIdAsync(It.IsAny<Guid>()))
                               .ReturnsAsync((CustomerProfile)null);


            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.MergeAndDeleteAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task NormalizePhone_VariousFormats_ReturnsCorrectFormat()
        {
            
            var profileId = Guid.NewGuid();
            var current = new CustomerProfile { Id = profileId, IsProfileCompleted = false };
            var dto = new EnrichCustomerRequest { ActiveCustomerId = profileId, Phone = "+84 909.123-456" };

            _mockProfileService.Setup(s => s.GetCustomerProfileByIdAsync(profileId)).ReturnsAsync(current);
            _mockCustomerRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<CustomerProfile, bool>>>(), null, null))
                             .ReturnsAsync((CustomerProfile)null);

          
            await _service.HandleEnrichCustomerAsync(dto);

            
            Assert.Equal("0909123456", current.PhoneNumber);
        }
    }
}
