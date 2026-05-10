using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
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
using System.Net;
using SystemClaim = System.Security.Claims.Claim;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.SupportStaffMessServiceTest
{
    public class SendSystemMessageTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<SupportStaffMessage>> _staffMsgRepoMock;
        private readonly Mock<IGenericRepository<SupportConversation>> _conversationRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IZaloOAuthService> _zaloOAuthMock;
        private readonly Mock<ICustomerProfileService> _customerProfileMock;
        private readonly Mock<ISupportConversationService> _conversationServiceMock;
        private readonly Mock<IChatTemplateService> _chatTemplateMock;
        private readonly Mock<IProviderService> _providerMock;
        private readonly Mock<IHubContext<SupportConversationHub>> _hubContextMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly SupportStaffMessageService _service;

        private readonly Guid _zaloProviderId = Guid.Parse("bb4a4a44-4b03-442f-9a5e-a43ad45391a0");
        private readonly Guid _facebookProviderId = Guid.Parse("67c4f1fd-9612-4a22-a30d-809b1598455b");

        public SendSystemMessageTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _staffMsgRepoMock = new Mock<IGenericRepository<SupportStaffMessage>>();
            _conversationRepoMock = new Mock<IGenericRepository<SupportConversation>>();
            _mapperMock = new Mock<IMapper>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _zaloOAuthMock = new Mock<IZaloOAuthService>();
            _customerProfileMock = new Mock<ICustomerProfileService>();
            _conversationServiceMock = new Mock<ISupportConversationService>();
            _chatTemplateMock = new Mock<IChatTemplateService>();
            _providerMock = new Mock<IProviderService>();
            _hubContextMock = new Mock<IHubContext<SupportConversationHub>>();
            _configurationMock = new Mock<IConfiguration>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var user = new ClaimsPrincipal(new ClaimsIdentity(new SystemClaim[] {
                new SystemClaim(ClaimTypes.NameIdentifier, Guid.Empty.ToString())
            }));
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.User).Returns(user);
            _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(mockContext.Object);

            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _hubContextMock.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockClientProxy.Object);
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _service = new SupportStaffMessageService(
                _unitOfWorkMock.Object,
                new Mock<ILogger<SupportStaffMessageService>>().Object,
                _mapperMock.Object,
                _httpContextAccessorMock.Object,
                httpClient,
                _zaloOAuthMock.Object,
                _customerProfileMock.Object,
                _conversationServiceMock.Object,
                _configurationMock.Object,
                _hubContextMock.Object,
                _providerMock.Object,
                _chatTemplateMock.Object
            );
        }

        [Fact]
        public async Task SendSystemMessageToExternalAsync_ZaloProvider_CallsZaloFlow()
        {
            var conversationId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var conversation = new SupportConversation
            {
                Id = conversationId,
                ProvidersId = _zaloProviderId,
                ActiveCustomerId = customerId
            };
            var staffMsg = new SupportStaffMessage
            {
                Id = Guid.NewGuid(),
                SupportConversationId = conversationId,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_conversationRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<SupportStaffMessage>()).Returns(_staffMsgRepoMock.Object);

            _conversationRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(conversation);

            _mapperMock.Setup(m => m.Map<SupportStaffMessage>(It.IsAny<CreateSupportStaffMessageRequest>())).Returns(staffMsg);


            _conversationServiceMock.Setup(s => s.GetSupportConversationByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(conversation);

            _customerProfileMock.Setup(s => s.GetCustomerProfileByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new CustomerProfile { ZaloSenderId = "ZALO_123" });

            _zaloOAuthMock.Setup(s => s.GetAccessTokenAsync()).ReturnsAsync("token");
            _chatTemplateMock.Setup(s => s.ExpandTemplateCodesAsync(It.IsAny<string>())).ReturnsAsync("Guide message");
            _providerMock.Setup(s => s.GetProviderByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Provider { ProviderName = "Zalo" });

            await _service.SendSystemMessageToExternalAsync(conversationId, "Guide content");

            _zaloOAuthMock.Verify(s => s.GetAccessTokenAsync(), Times.Once);
        }

        [Fact]
        public async Task SendSystemMessageToExternalAsync_FacebookProvider_CallsFacebookFlow()
        {
            var conversationId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var conversation = new SupportConversation
            {
                Id = conversationId,
                ProvidersId = _facebookProviderId,
                ActiveCustomerId = customerId
            };
            var staffMsg = new SupportStaffMessage
            {
                Id = Guid.NewGuid(),
                SupportConversationId = conversationId,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_conversationRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.GetRepository<SupportStaffMessage>()).Returns(_staffMsgRepoMock.Object);
            _conversationRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(conversation);

            _mapperMock.Setup(m => m.Map<SupportStaffMessage>(It.IsAny<CreateSupportStaffMessageRequest>())).Returns(staffMsg);

            _conversationServiceMock.Setup(s => s.GetSupportConversationByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(conversation);

            _customerProfileMock.Setup(s => s.GetCustomerProfileByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new CustomerProfile { FacebookSenderId = "FB_123" });

            _configurationMock.Setup(c => c["facebookWebHook:AccessToken"]).Returns("fb_token");
            _chatTemplateMock.Setup(s => s.ExpandTemplateCodesAsync(It.IsAny<string>())).ReturnsAsync("Guide message");
            _providerMock.Setup(s => s.GetProviderByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Provider { ProviderName = "Facebook" });

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

            await _service.SendSystemMessageToExternalAsync(conversationId, "Guide content");

            _configurationMock.Verify(c => c["facebookWebHook:AccessToken"], Times.AtLeastOnce);
        }
    }
}
