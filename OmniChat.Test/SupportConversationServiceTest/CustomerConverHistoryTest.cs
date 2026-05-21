using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using MockQueryable.Moq;
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
    public class CustomerConverHistoryTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<SupportConversation>> _repoMock;
        private readonly Mock<ICustomerProfileService> _customerProfileServiceMock;
        private readonly SupportConversationService _service;

        public CustomerConverHistoryTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _repoMock = new Mock<IGenericRepository<SupportConversation>>();
            _customerProfileServiceMock = new Mock<ICustomerProfileService>();

            _service = new SupportConversationService(
                _unitOfWorkMock.Object,
                null, null, null,
                _customerProfileServiceMock.Object,
                new Mock<IHubContext<SidebarHub>>().Object,
                new Mock<ISupportTaskService>().Object,
                new Mock<INotificationService>().Object
            );
        }

        [Fact]
        public async Task GetCustomerConversationHistoryAsync_ValidId_ReturnsDetail()
        {
            var conversationId = Guid.NewGuid();
            var customerId = Guid.NewGuid();

            var data = new List<SupportConversation>
            {
                new SupportConversation
                {
                    Id = conversationId,
                    Status = ConversationStatus.Complete,
                    ActiveCustomerId = customerId,
                    CustomerName = "Test Customer",
                    SupportTasks = new List<SupportTask> { new SupportTask { Status = SupportTaskStatus.Done } },
                    CustomerMessages = new List<CustomerMessage> { new CustomerMessage { Content = "Msg 1", Timestamp = 100 } },
                    SupportStaffMessages = new List<SupportStaffMessage> { new SupportStaffMessage { Content = "Msg 2", Timestamp = 200, StaffId = Guid.NewGuid() } }
                }
            }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);

            _repoMock.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IQueryable<SupportConversation>>>(),
                It.IsAny<bool>()
            )).Returns(data);

            _customerProfileServiceMock.Setup(s => s.GetCustomerProfileByIdAsync(customerId))
                .ReturnsAsync(new CustomerProfile { Id = customerId });

            var result = await _service.GetCustomerConversationHistoryAsync(conversationId);

            result.Should().NotBeNull();
            result.Id.Should().Be(conversationId);
            result.Messages.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetCustomerConversationHistoryAsync_NotFound_ThrowsException()
        {
            var data = new List<SupportConversation>().AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);

            _repoMock.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IQueryable<SupportConversation>>>(),
                It.IsAny<bool>()
            )).Returns(data);

            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetCustomerConversationHistoryAsync(Guid.NewGuid()));
        }
    }
}
