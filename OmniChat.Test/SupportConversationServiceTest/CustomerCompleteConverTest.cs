using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using MockQueryable.Moq;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
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
    public class CustomerCompleteConverTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<SupportConversation>> _repoMock;
        private readonly SupportConversationService _service;

        public CustomerCompleteConverTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _repoMock = new Mock<IGenericRepository<SupportConversation>>();
            var taskAssignment = new Mock<ITaskAssignmentService>();
            _service = new SupportConversationService(
                _unitOfWorkMock.Object,
                null, null, null,
                new Mock<ICustomerProfileService>().Object,
                new Mock<IHubContext<SidebarHub>>().Object,
                new Mock<ISupportTaskService>().Object,
                new Mock<INotificationService>().Object,
                taskAssignment.Object
            );
        }

        [Fact]
        public async Task GetCustomerCompleteSupportConversationHistoryAsync_ValidCustomerId_ReturnsHistory()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();

            var data = new List<SupportConversation>
            {
                new SupportConversation
                {
                    Id = conversationId,
                    ActiveCustomerId = customerId,
                    Status = ConversationStatus.Complete,
                    Providers = new Provider { ProviderName = "Facebook" },
                    SupportTasks = new List<SupportTask>
                    {
                        new SupportTask
                        {
                            Status = SupportTaskStatus.Done,
                            CompleteDate = DateTime.UtcNow,
                            IntentType = new IntentType { TypeName = "Tư vấn" },
                            CurrentAssignedStaff = new Staff { Name = "Nhân viên hỗ trợ A" }
                        }
                    }
                }
            }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);

            _repoMock.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IQueryable<SupportConversation>>>(),
                It.IsAny<bool>()
            )).Returns(data);

            var result = await _service.GetCustomerCompleteSupportConversationHistoryAsync(customerId);

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().ProviderName.Should().Be("Facebook");
            result.First().KeywordType.Should().Be("Tư vấn");
            result.First().StaffName.Should().Be("Nhân viên hỗ trợ A");
        }

        [Fact]
        public async Task GetCustomerCompleteSupportConversationHistoryAsync_NoCompletedTask_ReturnsEmptyList()
        {
            var customerId = Guid.NewGuid();
            var data = new List<SupportConversation>
            {
                new SupportConversation
                {
                    ActiveCustomerId = customerId,
                    Status = ConversationStatus.Complete,
                    SupportTasks = new List<SupportTask>
                    {
                        new SupportTask { Status = SupportTaskStatus.InProgress }
                    }
                }
            }.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);
            _repoMock.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                null,
                false
            )).Returns(data);

            var result = await _service.GetCustomerCompleteSupportConversationHistoryAsync(customerId);

            result.Should().BeEmpty();
        }
    }
}
