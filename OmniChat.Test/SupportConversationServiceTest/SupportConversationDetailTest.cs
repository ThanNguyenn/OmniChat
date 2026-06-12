using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
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

namespace OmniChat.Test.SupportConversationServiceTest
{
    public class SupportConversationDetailTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _unitOfWorkMock;
        private readonly Mock<ICustomerProfileService> _customerProfileServiceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IHubContext<SidebarHub>> _hubContextMock;
        private readonly Mock<IGenericRepository<SupportConversation>> _repoMock;
        private readonly SupportConversationService _service;

        public SupportConversationDetailTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _customerProfileServiceMock = new Mock<ICustomerProfileService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _hubContextMock = new Mock<IHubContext<SidebarHub>>();
            _repoMock = new Mock<IGenericRepository<SupportConversation>>();
            var taskAssignment = new Mock<ITaskAssignmentService>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _hubContextMock.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockClientProxy.Object);

            _service = new SupportConversationService(
                _unitOfWorkMock.Object,
                null,
                null, 
                null, 
                _customerProfileServiceMock.Object,
                _hubContextMock.Object,
                null, 
                _notificationServiceMock.Object,
                taskAssignment.Object
            );
        }

        [Fact]
        public async Task GetConversationDetailByIdAsync_ValidId_ReturnsDetailResponse()
        {

            var conversationId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var fakeTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var conversation = new SupportConversation
            {
                Id = conversationId,
                ActiveStaffId = staffId,
                ActiveCustomerId = customerId,
                CustomerName = "Test Customer",
                CustomerMessages = new List<CustomerMessage>
        {
            new CustomerMessage
            {
                Content = "Hello",
                Timestamp = fakeTimestamp - 1000, 
                CustomerId = customerId
            }
        },
                SupportStaffMessages = new List<SupportStaffMessage>
        {
            new SupportStaffMessage
            {
                StaffId = staffId,
                Content = "Hi",
                Timestamp = fakeTimestamp,
                Status = SupportStaffMessageStatus.Sent
            }
        },
                Providers = new Provider { ProviderName = "Zalo" }
            };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);
            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IOrderedQueryable<SupportConversation>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()
            )).ReturnsAsync(conversation);

         
            var customerMsgRepoMock = new Mock<IGenericRepository<CustomerMessage>>();
            _unitOfWorkMock.Setup(u => u.GetRepository<CustomerMessage>()).Returns(customerMsgRepoMock.Object);

       
            customerMsgRepoMock.Setup(r => r.UpdateRange(It.IsAny<IEnumerable<CustomerMessage>>()));

          
            _customerProfileServiceMock.Setup(s => s.GetCustomerProfileByIdAsync(customerId))
                .ReturnsAsync(new CustomerProfile { Id = customerId });

            var result = await _service.GetConversationDetailByIdAsync(conversationId);

            result.Should().NotBeNull();
            result.Id.Should().Be(conversationId);
            result.Messages.Should().HaveCount(2);

            _notificationServiceMock.Verify(n => n.UpdateNotificationIsReadAsync(conversationId), Times.Once);
        }

        [Fact]
        public async Task GetConversationDetailByIdAsync_InvalidId_ThrowsNotFoundException()
        {
            var conversationId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);

            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IOrderedQueryable<SupportConversation>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()
            )).ReturnsAsync((SupportConversation)null);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetConversationDetailByIdAsync(conversationId));
        }
    }
}
