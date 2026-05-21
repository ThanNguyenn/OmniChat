using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.SupportConversationServiceTest
{
    public class ConversationSideBarTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<SupportConversation>> _repoMock;
        private readonly SupportConversationService _service;

        public ConversationSideBarTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _repoMock = new Mock<IGenericRepository<SupportConversation>>();

            var customerProfileMock = new Mock<ICustomerProfileService>();
            var hubContextMock = new Mock<IHubContext<SidebarHub>>();
            var notificationMock = new Mock<INotificationService>();
            var supportTaskMock = new Mock<ISupportTaskService>();

            _service = new SupportConversationService(
                _unitOfWorkMock.Object,
                null, 
                null, 
                null, 
                customerProfileMock.Object,
                hubContextMock.Object,
                supportTaskMock.Object,
                notificationMock.Object
            );
        }

        [Fact]
        public async Task GetStaffConversationSideBarAsync_ReturnsListResponse()
        {
            var staffId = Guid.NewGuid();
            var providerName = "Zalo";

            var mockData = new List<StaffConversationSideBarResponse>
    {
        new StaffConversationSideBarResponse
        {
            ConversationId = Guid.NewGuid(),
            CustomerName = "Khách hàng A",
            ProviderName = "Zalo",
            LastMessage = "Tin nhắn cuối",
            UnreadMessageCount = 1,
            UpdateDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        }
    };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);

            _repoMock.Setup(r => r.GetListAsync<StaffConversationSideBarResponse>(
                It.IsAny<Expression<Func<SupportConversation, StaffConversationSideBarResponse>>>(), 
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),                          
                It.IsAny<Func<IQueryable<SupportConversation>, IOrderedQueryable<SupportConversation>>>(), 
                It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()
            )).ReturnsAsync(mockData);

            
            var result = await _service.GetStaffConversationSideBarAsync(staffId, providerName);

          
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().CustomerName.Should().Be("Khách hàng A");
        }

        [Fact]
        public async Task GetStaffConversationSideBarAsync_WhenNoData_ReturnsEmptyList()
        {
           
            var staffId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);

          
            _repoMock.Setup(r => r.GetListAsync<StaffConversationSideBarResponse>(
                It.IsAny<Expression<Func<SupportConversation, StaffConversationSideBarResponse>>>(), 
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),                            
                It.IsAny<Func<IQueryable<SupportConversation>, IOrderedQueryable<SupportConversation>>>(), 
                It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>() 
            )).ReturnsAsync(new List<StaffConversationSideBarResponse>());

            var result = await _service.GetStaffConversationSideBarAsync(staffId, null);

            result.Should().BeEmpty();
        }
    }
}
