using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.TaskAction;
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
    public class GetAllTaskActionTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<TaskAction>> _taskActionRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<TaskActionService>> _loggerMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly TaskActionService _service;

        public GetAllTaskActionTest()
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
        public async Task GetAllTaskActionAsync_ValidPagination_ReturnsPagingResponse()
        {
            int pageIndex = 1;
            int pageSize = 10;

            var data = new List<TaskAction>
    {
        new TaskAction { Id = Guid.NewGuid(), CreateDate = DateTime.UtcNow.AddDays(-1) },
        new TaskAction { Id = Guid.NewGuid(), CreateDate = DateTime.UtcNow }
    };

            var mockQueryable = data.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(u => u.GetRepository<TaskAction>()).Returns(_taskActionRepoMock.Object);

            _taskActionRepoMock.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<TaskAction, bool>>>(),
                It.IsAny<Func<IQueryable<TaskAction>, IQueryable<TaskAction>>>(),
                It.IsAny<bool>()
            )).Returns(mockQueryable);

            var mappedData = new List<TaskActionResponse>
    {
        new TaskActionResponse { Id = data[0].Id },
        new TaskActionResponse { Id = data[1].Id }
    };

            _mapperMock.Setup(m => m.Map<IEnumerable<TaskActionResponse>>(It.IsAny<List<TaskAction>>()))
                       .Returns(mappedData);

            var result = await _service.GetAllTaskActionAsync(pageIndex, pageSize);

            result.Should().NotBeNull();
            result.Meta.TotalItems.Should().Be(2);
            _taskActionRepoMock.Verify(r => r.GetQueryable(null, null, false), Times.Once);
        }
    }
}
