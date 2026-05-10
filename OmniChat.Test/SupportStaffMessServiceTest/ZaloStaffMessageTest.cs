using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SystemClaim = System.Security.Claims.Claim;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.SupportStaffMessServiceTest
{
    public class ZaloStaffMessageTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<SupportStaffMessage>> _staffMsgRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IZaloOAuthService> _zaloOAuthMock;
        private readonly Mock<ICustomerProfileService> _customerProfileMock;
        private readonly Mock<ISupportConversationService> _conversationServiceMock;
        private readonly Mock<IChatTemplateService> _chatTemplateMock;
        private readonly Mock<IProviderService> _providerMock;
        private readonly Mock<IHubContext<SupportConversationHub>> _hubContextMock;
        private readonly SupportStaffMessageService _service;

        public ZaloStaffMessageTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _staffMsgRepoMock = new Mock<IGenericRepository<SupportStaffMessage>>();
            _mapperMock = new Mock<IMapper>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _zaloOAuthMock = new Mock<IZaloOAuthService>();
            _customerProfileMock = new Mock<ICustomerProfileService>();
            _conversationServiceMock = new Mock<ISupportConversationService>();
            _chatTemplateMock = new Mock<IChatTemplateService>();
            _providerMock = new Mock<IProviderService>();
            _hubContextMock = new Mock<IHubContext<SupportConversationHub>>();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new SystemClaim[] {
                new SystemClaim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.User).Returns(user);
            _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(mockContext.Object);

            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _hubContextMock.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockClientProxy.Object);
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

            _service = new SupportStaffMessageService(
                _unitOfWorkMock.Object,
                new Mock<ILogger<SupportStaffMessageService>>().Object,
                _mapperMock.Object,
                _httpContextAccessorMock.Object,
                new HttpClient(),
                _zaloOAuthMock.Object,
                _customerProfileMock.Object,
                _conversationServiceMock.Object,
                new Mock<IConfiguration>().Object,
                _hubContextMock.Object,
                _providerMock.Object,
                _chatTemplateMock.Object
            );
        }

        [Fact]
        public async Task SendZaloMessageAsync_ValidRequest_ReturnsTrue()
        {
            var request = new CreateSupportStaffMessageRequest
            {
                SupportConversationId = Guid.NewGuid(),
                Content = "Hello CODE01"
            };

            var staffMsg = new SupportStaffMessage
            {
                Id = Guid.NewGuid(),
                SupportConversationId = request.SupportConversationId,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            var conversation = new SupportConversation
            {
                Id = request.SupportConversationId,
                ActiveCustomerId = Guid.NewGuid(),
                ActiveStaffId = Guid.NewGuid(),
                ProvidersId = Guid.NewGuid(),
                AvatarUrl = "http://avatar.com"
            };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportStaffMessage>()).Returns(_staffMsgRepoMock.Object);

            _mapperMock.Setup(m => m.Map<SupportStaffMessage>(It.IsAny<CreateSupportStaffMessageRequest>()))
                .Returns(staffMsg);

            _conversationServiceMock.Setup(s => s.GetSupportConversationByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(conversation);

            _customerProfileMock.Setup(s => s.GetCustomerProfileByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new CustomerProfile { ZaloSenderId = "UID_123" });

            _zaloOAuthMock.Setup(s => s.GetAccessTokenAsync()).ReturnsAsync("fake_token");

            _chatTemplateMock.Setup(s => s.ExpandTemplateCodesAsync(It.IsAny<string>()))
                .ReturnsAsync("Expanded Content");

            _providerMock.Setup(s => s.GetProviderByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new Provider { ProviderName = "Zalo" });

           
            var result = await _service.SendZaloMessageAsync(request);

            result.Should().BeTrue();
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.AtLeastOnce);
        }
    }
}
