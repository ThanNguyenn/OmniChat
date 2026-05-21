using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Requests.Claim;
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

namespace OmniChat.Test.ClaimServiceTest
{
    public class CreateClaimTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ClaimService>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<ITaskActionService> _mockTaskAction;
        private readonly Mock<IHubContext<SupportConversationHub>> _mockHub;

        private readonly Mock<IGenericRepository<Claim>> _mockClaimRepo;
        private readonly Mock<IGenericRepository<ClaimType>> _mockClaimTypeRepo;
        private readonly Mock<IGenericRepository<SupportTask>> _mockTaskRepo;
        private readonly Mock<IGenericRepository<SupportConversation>> _mockConvRepo;
        private readonly ClaimService _service;

        public CreateClaimTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ClaimService>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();
            _mockTaskAction = new Mock<ITaskActionService>();
            _mockHub = new Mock<IHubContext<SupportConversationHub>>();

            _mockClaimRepo = new Mock<IGenericRepository<Claim>>();
            _mockClaimTypeRepo = new Mock<IGenericRepository<ClaimType>>();
            _mockTaskRepo = new Mock<IGenericRepository<SupportTask>>();
            _mockConvRepo = new Mock<IGenericRepository<SupportConversation>>();

            _mockUow.Setup(u => u.GetRepository<Claim>()).Returns(_mockClaimRepo.Object);
            _mockUow.Setup(u => u.GetRepository<ClaimType>()).Returns(_mockClaimTypeRepo.Object);
            _mockUow.Setup(u => u.GetRepository<SupportTask>()).Returns(_mockTaskRepo.Object);
            _mockUow.Setup(u => u.GetRepository<SupportConversation>()).Returns(_mockConvRepo.Object);

            _mockUow.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
            .Returns((Func<Task<bool>> func) => func());

            _service = new ClaimService(
                _mockUow.Object,
                _mockLogger.Object,
                _mockMapper.Object,
                _mockAccessor.Object,
                _mockTaskAction.Object,
                _mockHub.Object);
        }

        [Fact]
        public async Task CreateClaimAsync_NormalType_ReturnsTrue()
        {
            // Arrange
            var request = new CreateClaimRequest { ClaimTypeId = Guid.NewGuid() };
            var claimType = new ClaimType { Id = request.ClaimTypeId };

            _mockClaimTypeRepo.Setup(r => r.GetByIdAsync(request.ClaimTypeId)).ReturnsAsync(claimType);
            _mockMapper.Setup(m => m.Map<Claim>(request)).Returns(new Claim());

            // Act
            var result = await _service.CreateClaimAsync(request);

            // Assert
            Assert.True(result);
            _mockClaimRepo.Verify(r => r.InsertAsync(It.IsAny<Claim>()), Times.Once);
        }

        [Fact]
        public async Task CreateClaimAsync_ClaimTypeNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var request = new CreateClaimRequest { ClaimTypeId = Guid.NewGuid() };
            _mockClaimTypeRepo.Setup(r => r.GetByIdAsync(request.ClaimTypeId)).ReturnsAsync((ClaimType)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateClaimAsync(request));
        }

        [Fact]
        public async Task CreateClaimAsync_ChangeTaskMissingConvId_ThrowsBadRequestException()
        {
            // Arrange
            var changeTaskTypeId = Guid.Parse("abf8b2a1-0699-4c27-b241-11df7a75c12c");
            var request = new CreateClaimRequest { ClaimTypeId = changeTaskTypeId, SupportConversationId = null };

            _mockClaimTypeRepo.Setup(r => r.GetByIdAsync(changeTaskTypeId)).ReturnsAsync(new ClaimType { Id = changeTaskTypeId });

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateClaimAsync(request));
        }

        [Fact]
        public async Task CreateClaimAsync_ChangeTask_UpdatesStatusAndReturnsTrue()
        {
            // Arrange
            var changeTaskTypeId = Guid.Parse("abf8b2a1-0699-4c27-b241-11df7a75c12c");
            var convId = Guid.NewGuid();

            // SupportConversationId là string, truyền convId.ToString()
            var request = new CreateClaimRequest
            {
                ClaimTypeId = changeTaskTypeId,
                SupportConversationId = convId.ToString()
            };

            var conversation = new SupportConversation
            {
                Id = convId,
                SupportTasks = new List<SupportTask>
        {
            new SupportTask { Status = SupportTaskStatus.InProgress }
        }
            };

            _mockClaimTypeRepo.Setup(r => r.GetByIdAsync(changeTaskTypeId))
                .ReturnsAsync(new ClaimType { Id = changeTaskTypeId });

            _mockConvRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IOrderedQueryable<SupportConversation>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()
            ))
            .ReturnsAsync(conversation);

            _mockMapper.Setup(m => m.Map<Claim>(request)).Returns(new Claim());

            // Act
            var result = await _service.CreateClaimAsync(request);

            // Assert
            Assert.True(result);
            Assert.Equal(ConversationStatus.PendingReassign, conversation.Status);
            Assert.Equal(SupportTaskStatus.PendingReassign, conversation.SupportTasks.First().Status);
            _mockConvRepo.Verify(r => r.Update(conversation), Times.Once);
            _mockTaskRepo.Verify(r => r.UpdateRange(It.IsAny<IEnumerable<SupportTask>>()), Times.Once);
        }
    }
}
