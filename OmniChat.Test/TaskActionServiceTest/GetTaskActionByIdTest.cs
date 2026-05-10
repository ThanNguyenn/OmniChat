using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.TaskAction;
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

namespace OmniChat.Test.TaskActionServiceTest
{
    public class GetTaskActionByIdTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<TaskAction>> _taskActionRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<TaskActionService>> _loggerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly TaskActionService _service;

        public GetTaskActionByIdTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _taskActionRepoMock = new Mock<IGenericRepository<TaskAction>>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<TaskActionService>>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _service = new TaskActionService(
                _unitOfWorkMock.Object,
                _loggerMock.Object,
                _mapperMock.Object,
                _httpContextAccessorMock.Object
            );
        }
        [Fact]
        public async Task GetTaskActionByIdAsync_ExistingId_ReturnsTaskActionResponse()
        {
            var actionId = Guid.NewGuid();
            var taskAction = new TaskAction { Id = actionId, Reason = "Test Action" };
            var response = new TaskActionResponse { Id = actionId, Reason = "Test Action" };

            _unitOfWorkMock.Setup(u => u.GetRepository<TaskAction>()).Returns(_taskActionRepoMock.Object);

            _taskActionRepoMock.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<TaskAction, bool>>>(), 
                null, 
                null  
            )).ReturnsAsync(taskAction);

            _mapperMock.Setup(m => m.Map<TaskActionResponse>(taskAction)).Returns(response);

            var result = await _service.GetTaskActionByIdAsync(actionId);

            result.Should().NotBeNull();
            _taskActionRepoMock.Verify(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<TaskAction, bool>>>(), null, null), Times.Once);
        }

        [Fact]
        public async Task GetTaskActionByIdAsync_NonExistingId_ThrowsNotFoundException()
        {
            var actionId = Guid.NewGuid();
            _unitOfWorkMock.Setup(u => u.GetRepository<TaskAction>()).Returns(_taskActionRepoMock.Object);

            _taskActionRepoMock.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<TaskAction, bool>>>(),
                null,
                null
            )).ReturnsAsync((TaskAction)null);

            Func<Task> act = async () => await _service.GetTaskActionByIdAsync(actionId);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
