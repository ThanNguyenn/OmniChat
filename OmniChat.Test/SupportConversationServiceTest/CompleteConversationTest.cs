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
    public class CompleteConversationTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<SupportConversation>> _repoMock;
        private readonly Mock<ISupportTaskService> _supportTaskServiceMock;
        private readonly SupportConversationService _service;
        private readonly Mock<ITaskAssignmentService> _taskAssignmentServiceMock;

        public CompleteConversationTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _repoMock = new Mock<IGenericRepository<SupportConversation>>();
            _supportTaskServiceMock = new Mock<ISupportTaskService>();

            _service = new SupportConversationService(
                _unitOfWorkMock.Object,
                null, null, null,
                new Mock<ICustomerProfileService>().Object,
                new Mock<IHubContext<SidebarHub>>().Object,
                _supportTaskServiceMock.Object,
                new Mock<INotificationService>().Object,
                new Mock<ITaskAssignmentService>().Object
            );
        }

        [Fact]
        public async Task CompleteConversationAsync_ValidData_ReturnsTrue()
        {
            var conversationId = Guid.NewGuid();
            var conversation = new SupportConversation
            {
                Id = conversationId,
                Status = ConversationStatus.Pending 
            };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);
            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IOrderedQueryable<SupportConversation>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()
            )).ReturnsAsync(conversation);

            var tasks = new List<SupportTask>
            {
                new SupportTask { Status = SupportTaskStatus.Done },
                new SupportTask { Status = SupportTaskStatus.Done }
            };
            _supportTaskServiceMock.Setup(s => s.GetSupportTaskByConversationIdAsync(conversationId))
                .ReturnsAsync(tasks);

            var result = await _service.CompleteConversationAsync(conversationId);

            result.Should().BeTrue();
            conversation.Status.Should().Be(ConversationStatus.Complete);
            conversation.CloseAt.Should().NotBeNull();

            _repoMock.Verify(r => r.Update(conversation), Times.Once);
            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CompleteConversationAsync_AlreadyCompleted_ThrowsBadRequestException()
        {
            var conversationId = Guid.NewGuid();
            var conversation = new SupportConversation
            {
                Id = conversationId,
                Status = ConversationStatus.Complete
            };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);

            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IOrderedQueryable<SupportConversation>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()
            )).ReturnsAsync(conversation);

            var act = async () => await _service.CompleteConversationAsync(conversationId);

            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Cuộc trò chuyện này đã được hoàn thành trước đó");
        }

        [Fact]
        public async Task CompleteConversationAsync_TasksNotDone_ThrowsBadRequestException()
        {
            var conversationId = Guid.NewGuid();
            var conversation = new SupportConversation
            {
                Id = conversationId,
                Status = ConversationStatus.Pending
            };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);

            _repoMock.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IOrderedQueryable<SupportConversation>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()
            )).ReturnsAsync(conversation);

            var tasks = new List<SupportTask>
    {
        new SupportTask { Status = SupportTaskStatus.InProgress }
    };
            _supportTaskServiceMock.Setup(s => s.GetSupportTaskByConversationIdAsync(conversationId))
                .ReturnsAsync(tasks);

            var act = async () => await _service.CompleteConversationAsync(conversationId);

            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Chưa hoàn thành hết yêu cầu hỗ trợ");
        }
    }
}
