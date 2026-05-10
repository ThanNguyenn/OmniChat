using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Responses.SupportStaffMessage;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.SupportStaffMessServiceTest
{
    public class GetAllStaffSupportMessageTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<SupportStaffMessage>> _repoMock;
        private readonly SupportStaffMessageService _service;

        public GetAllStaffSupportMessageTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _repoMock = new Mock<IGenericRepository<SupportStaffMessage>>();

            var httpClientMock = new Mock<HttpClient>();
            var configMock = new Mock<IConfiguration>();
            var zaloOAuthMock = new Mock<IZaloOAuthService>();
            var customerProfileMock = new Mock<ICustomerProfileService>();
            var conversationMock = new Mock<ISupportConversationService>();
            var providerMock = new Mock<IProviderService>();
            var chatTemplateMock = new Mock<IChatTemplateService>();
            var hubContextMock = new Mock<IHubContext<SupportConversationHub>>();
            var loggerMock = new Mock<ILogger<SupportStaffMessageService>>();

            _service = new SupportStaffMessageService(
                _unitOfWorkMock.Object,
                loggerMock.Object,
                null, 
                null, 
                httpClientMock.Object,
                zaloOAuthMock.Object,
                customerProfileMock.Object,
                conversationMock.Object,
                configMock.Object,
                hubContextMock.Object,
                providerMock.Object,
                chatTemplateMock.Object
            );
        }

        [Fact]
        public async Task GetAllSupportStaffMessageByStaffIdAsync_WithStaffId_ReturnsPagingResponse()
        {
            var staffId = Guid.NewGuid();
            int page = 1;
            int size = 20;

            var mockData = new List<GetAllSupportStaffMessageResponse>
            {
                new GetAllSupportStaffMessageResponse { Id = Guid.NewGuid(), Content = "Hello", StaffId = staffId }
            };

            var pagingResponse = new PagingResponse<GetAllSupportStaffMessageResponse>
            {
                Items = mockData,
                Meta = new PaginationMeta
                {
                    CurrentPage = page,
                    PageSize = size,
                    TotalItems = 1
                }
            };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportStaffMessage>()).Returns(_repoMock.Object);

            _repoMock.Setup(r => r.GetPagingListAsync<GetAllSupportStaffMessageResponse>(
                It.IsAny<Expression<Func<SupportStaffMessage, GetAllSupportStaffMessageResponse>>>(), 
                It.IsAny<Expression<Func<SupportStaffMessage, bool>>>(),                            
                It.IsAny<Func<IQueryable<SupportStaffMessage>, IOrderedQueryable<SupportStaffMessage>>>(),
                null, 
                page,
                size
            )).ReturnsAsync(pagingResponse);

            var result = await _service.GetAllSupportStaffMessageByStaffIdAsync(page, size, staffId);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.Items.First().Content.Should().Be("Hello");
            result.Meta.TotalItems.Should().Be(1);
        }

        [Fact]
        public async Task GetAllSupportStaffMessageByStaffIdAsync_StaffIdNull_ReturnsAllMessages()
        {
            var pagingResponse = new PagingResponse<GetAllSupportStaffMessageResponse>
            {
                Items = new List<GetAllSupportStaffMessageResponse>(),
                Meta = new PaginationMeta { TotalItems = 0 }
            };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportStaffMessage>()).Returns(_repoMock.Object);

            _repoMock.Setup(r => r.GetPagingListAsync<GetAllSupportStaffMessageResponse>(
                It.IsAny<Expression<Func<SupportStaffMessage, GetAllSupportStaffMessageResponse>>>(),
                null, 
                It.IsAny<Func<IQueryable<SupportStaffMessage>, IOrderedQueryable<SupportStaffMessage>>>(),
                null,
                It.IsAny<int>(),
                It.IsAny<int>()
            )).ReturnsAsync(pagingResponse);

            var result = await _service.GetAllSupportStaffMessageByStaffIdAsync(1, 20, null);

            result.Items.Should().BeEmpty();
            result.Meta.TotalItems.Should().Be(0);
        }
    }
}
