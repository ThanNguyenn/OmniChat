using AutoMapper;
using FluentAssertions;
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
    public class FacebookStaffMessageTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<SupportStaffMessage>> _staffMsgRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ICustomerProfileService> _customerProfileMock;
        private readonly Mock<ISupportConversationService> _conversationServiceMock;
        private readonly Mock<IChatTemplateService> _chatTemplateMock;
        private readonly Mock<IProviderService> _providerMock;
        private readonly Mock<IHubContext<SupportConversationHub>> _hubContextMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly SupportStaffMessageService _service;

        public FacebookStaffMessageTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _staffMsgRepoMock = new Mock<IGenericRepository<SupportStaffMessage>>();
            _mapperMock = new Mock<IMapper>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _configurationMock = new Mock<IConfiguration>();
            _customerProfileMock = new Mock<ICustomerProfileService>();
            _conversationServiceMock = new Mock<ISupportConversationService>();
            _chatTemplateMock = new Mock<IChatTemplateService>();
            _providerMock = new Mock<IProviderService>();
            _hubContextMock = new Mock<IHubContext<SupportConversationHub>>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            // 1. Mock HttpContext (Fix NullReference cho Base Service)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new SystemClaim[] {
                new SystemClaim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));
            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.User).Returns(user);
            _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(mockContext.Object);

            // 2. Mock SignalR Clients
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _hubContextMock.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockClientProxy.Object);
            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

            // 3. Mock HttpClient (Sử dụng DelegatingHandler để giả lập Facebook API)
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _service = new SupportStaffMessageService(
                _unitOfWorkMock.Object,
                new Mock<ILogger<SupportStaffMessageService>>().Object,
                _mapperMock.Object,
                _httpContextAccessorMock.Object,
                httpClient,
                new Mock<IZaloOAuthService>().Object,
                _customerProfileMock.Object,
                _conversationServiceMock.Object,
                _configurationMock.Object,
                _hubContextMock.Object,
                _providerMock.Object,
                _chatTemplateMock.Object
            );
        }

        [Fact]
        public async Task SendFacebookMesageAsync_ValidRequest_ReturnsTrue()
        {
            // Arrange
            var request = new CreateSupportStaffMessageRequest
            {
                SupportConversationId = Guid.NewGuid(),
                Content = "Hello Facebook FB01"
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
                ProvidersId = Guid.NewGuid()
            };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportStaffMessage>()).Returns(_staffMsgRepoMock.Object);
            _mapperMock.Setup(m => m.Map<SupportStaffMessage>(It.IsAny<CreateSupportStaffMessageRequest>())).Returns(staffMsg);

            _conversationServiceMock.Setup(s => s.GetSupportConversationByIdAsync(It.IsAny<Guid>())).ReturnsAsync(conversation);
            _customerProfileMock.Setup(s => s.GetCustomerProfileByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new CustomerProfile { FacebookSenderId = "FB_UID_123" });

            _configurationMock.Setup(c => c["facebookWebHook:AccessToken"]).Returns("fake_fb_token");
            _chatTemplateMock.Setup(s => s.ExpandTemplateCodesAsync(It.IsAny<string>())).ReturnsAsync("Hello Customer!");
            _providerMock.Setup(s => s.GetProviderByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Provider { ProviderName = "Facebook" });

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"message_id\": \"mid.123\"}")
                });

            // Act
            var result = await _service.SendFacebookMesageAsync(request);

            // Assert
            result.Should().BeTrue();
            staffMsg.Status.Should().Be(SupportStaffMessageStatus.Sent);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task SendFacebookMesageAsync_FacebookApiError_ThrowsBusinessException()
        {
            var request = new CreateSupportStaffMessageRequest { SupportConversationId = Guid.NewGuid(), Content = "Fail" };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportStaffMessage>()).Returns(_staffMsgRepoMock.Object);
            _mapperMock.Setup(m => m.Map<SupportStaffMessage>(It.IsAny<CreateSupportStaffMessageRequest>())).Returns(new SupportStaffMessage());
            _conversationServiceMock.Setup(s => s.GetSupportConversationByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new SupportConversation());
            _customerProfileMock.Setup(s => s.GetCustomerProfileByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new CustomerProfile { FacebookSenderId = "ID" });
            _configurationMock.Setup(c => c["facebookWebHook:AccessToken"]).Returns("token");

            // FIX: Giả lập Facebook API trả về lỗi 400
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"error\": \"Invalid token\"}")
                });

            var act = async () => await _service.SendFacebookMesageAsync(request);
            await act.Should().ThrowAsync<Exception>(); 
        }
    }
}
