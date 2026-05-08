using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Requests.TaskAction;
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
    public class RejectReassignClaimTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<Claim>> _mockClaimRepo;
        private readonly Mock<IGenericRepository<SupportConversation>> _mockConverRepo;
        private readonly Mock<IGenericRepository<SupportTask>> _mockTaskRepo;
        private readonly Mock<IHubContext<SupportConversationHub>> _mockHub;
        private readonly Mock<ITaskActionService> _mockTaskAction;
        private readonly Mock<ILogger<ClaimService>> _mockLogger;

        private readonly ClaimService _service;

        public RejectReassignClaimTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockClaimRepo = new Mock<IGenericRepository<Claim>>();
            _mockConverRepo = new Mock<IGenericRepository<SupportConversation>>();
            _mockTaskRepo = new Mock<IGenericRepository<SupportTask>>();
            _mockHub = new Mock<IHubContext<SupportConversationHub>>();
            _mockTaskAction = new Mock<ITaskActionService>();
            _mockLogger = new Mock<ILogger<ClaimService>>();

           
            _mockUow.Setup(u => u.GetRepository<Claim>()).Returns(_mockClaimRepo.Object);
            _mockUow.Setup(u => u.GetRepository<SupportConversation>()).Returns(_mockConverRepo.Object);
            _mockUow.Setup(u => u.GetRepository<SupportTask>()).Returns(_mockTaskRepo.Object);

            
            _mockUow.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
                    .Returns((Func<Task> func) => func());

            
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHub.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockClientProxy.Object);

            _service = new ClaimService(
                _mockUow.Object,
                _mockLogger.Object,
                new Mock<AutoMapper.IMapper>().Object,
                new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object,
                _mockTaskAction.Object,
                _mockHub.Object);
        }

        [Fact]
        public async Task RejectReassignClaimAsync_Success_UpdatesToInProgress()
        {
           
            var claimId = Guid.NewGuid();
            var managerId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();

            var claim = new Claim { Id = claimId, SupportConversationId = conversationId, StaffId = staffId };
            var conversation = new SupportConversation
            {
                Id = conversationId,
                ActiveStaffId = staffId,
                SupportTasks = new List<SupportTask>
                {
                    new SupportTask { Id = Guid.NewGuid(), Status = SupportTaskStatus.PendingReassign }
                }
            };

            _mockClaimRepo.Setup(r => r.GetByIdAsync(claimId)).ReturnsAsync(claim);

            
            _mockConverRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()
            )).ReturnsAsync(conversation);

            _mockConverRepo.Setup(r => r.CountAsync(It.IsAny<Expression<Func<SupportConversation, bool>>>()))
                .ReturnsAsync(0);

          
            await _service.RejectReassignClaimAsync(claimId, managerId);

            
            Assert.Equal(ClaimStatus.Rejected, claim.Status);
            Assert.Equal(ConversationStatus.Pending, conversation.Status);
            Assert.Equal(SupportTaskStatus.InProgress, conversation.SupportTasks.First().Status);

            _mockClaimRepo.Verify(r => r.Update(claim), Times.Once);
            _mockConverRepo.Verify(r => r.Update(conversation), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
            _mockTaskAction.Verify(t => t.CreateTaskActionAsync(It.IsAny<TaskActionRequest>()), Times.Once);
        }

        [Fact]
        public async Task RejectReassignClaimAsync_ClaimNotFound_ThrowsNotFoundException()
        {
           
            _mockClaimRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Claim)null);

            
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.RejectReassignClaimAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task RejectReassignClaimAsync_HighWorkload_SetsWarningStatus()
        {
            
            var claim = new Claim { Id = Guid.NewGuid(), SupportConversationId = Guid.NewGuid() };
            var conversation = new SupportConversation
            {
                ActiveStaffId = Guid.NewGuid(),
                SupportTasks = new List<SupportTask>()
            };

            _mockClaimRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(claim);
            _mockConverRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<SupportConversation, bool>>>(), null, It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()))
                .ReturnsAsync(conversation);

           
            _mockConverRepo.Setup(r => r.CountAsync(It.IsAny<Expression<Func<SupportConversation, bool>>>()))
                .ReturnsAsync(5);

           
            await _service.RejectReassignClaimAsync(claim.Id, Guid.NewGuid());

            
            Assert.Equal(ConversationStatus.Warning, conversation.Status);
        }

        [Fact]
        public async Task RejectReassignClaimAsync_NoActiveStaff_ThrowsBadRequestException()
        {
            
            var claim = new Claim { Id = Guid.NewGuid(), SupportConversationId = Guid.NewGuid() };
            var conversation = new SupportConversation { ActiveStaffId = null }; 

            _mockClaimRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(claim);
            _mockConverRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<SupportConversation, bool>>>(), null, It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()))
                .ReturnsAsync(conversation);

          
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.RejectReassignClaimAsync(claim.Id, Guid.NewGuid()));
        }
    }
}
