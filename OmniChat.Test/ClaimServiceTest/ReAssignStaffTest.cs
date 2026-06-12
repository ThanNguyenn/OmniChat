using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Query;
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
    public class ReAssignStaffTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<Claim>> _mockClaimRepo;
        private readonly Mock<IGenericRepository<SupportConversation>> _mockConverRepo;
        private readonly Mock<IGenericRepository<Staff>> _mockStaffRepo;
        private readonly Mock<IGenericRepository<SupportTask>> _mockTaskRepo;
        private readonly Mock<IGenericRepository<StaffPerformance>> _mockPerfRepo;
        private readonly Mock<IHubContext<SupportConversationHub>> _mockHub;
        private readonly Mock<ITaskActionService> _mockTaskAction;
        private readonly Mock<ISupportConversationService> _mockConversationService;
        private readonly Mock<IMailService> _mockMailService;
        private readonly ClaimService _service;

        public ReAssignStaffTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockClaimRepo = new Mock<IGenericRepository<Claim>>();
            _mockConverRepo = new Mock<IGenericRepository<SupportConversation>>();
            _mockStaffRepo = new Mock<IGenericRepository<Staff>>();
            _mockTaskRepo = new Mock<IGenericRepository<SupportTask>>();
            _mockPerfRepo = new Mock<IGenericRepository<StaffPerformance>>();
            _mockHub = new Mock<IHubContext<SupportConversationHub>>(); _mockConversationService = new Mock<ISupportConversationService>();
            _mockMailService = new Mock<IMailService>();
            _mockTaskAction = new Mock<ITaskActionService>();

          
            _mockUow.Setup(u => u.GetRepository<Claim>()).Returns(_mockClaimRepo.Object);
            _mockUow.Setup(u => u.GetRepository<SupportConversation>()).Returns(_mockConverRepo.Object);
            _mockUow.Setup(u => u.GetRepository<Staff>()).Returns(_mockStaffRepo.Object);
            _mockUow.Setup(u => u.GetRepository<SupportTask>()).Returns(_mockTaskRepo.Object);
            _mockUow.Setup(u => u.GetRepository<StaffPerformance>()).Returns(_mockPerfRepo.Object);

         
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHub.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.User(It.IsAny<string>())).Returns(mockClientProxy.Object);

            _service = new ClaimService(
                _mockUow.Object,
                new Mock<Microsoft.Extensions.Logging.ILogger<ClaimService>>().Object,
                new Mock<AutoMapper.IMapper>().Object,
                new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object,
                _mockTaskAction.Object, _mockMailService.Object,
_mockConversationService.Object,
                _mockHub.Object);
        }

        [Fact]
        public async Task ReAssignStaffAsync_Success_UpdatesAllEntities()
        {
            
            var claimId = Guid.NewGuid();
            var conversationId = Guid.NewGuid();
            var oldStaffId = Guid.NewGuid();
            var newStaffId = Guid.NewGuid();

            var claim = new Claim { Id = claimId, Status = ClaimStatus.Pending, Reason = "Nghỉ ốm" };
            var conversation = new SupportConversation
            {
                Id = conversationId,
                ActiveStaffId = oldStaffId,
                SupportTasks = new List<SupportTask>
                {
                    new SupportTask { Id = Guid.NewGuid(), Status = SupportTaskStatus.InProgress }
                }
            };
            var newStaff = new Staff { Id = newStaffId };
            var performance = new StaffPerformance { StaffId = oldStaffId, ReassignmentCount = 0 };

            
            _mockClaimRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<Claim, bool>>>(), null, null))
                .ReturnsAsync(claim);

            _mockConverRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<SupportConversation, bool>>>(), null, It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()))
                .ReturnsAsync(conversation);

            _mockStaffRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<Staff, bool>>>(), null, It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
                .ReturnsAsync(newStaff);

            _mockConverRepo.Setup(r => r.CountAsync(It.IsAny<Expression<Func<SupportConversation, bool>>>()))
                .ReturnsAsync(2); // Workload < 5

            _mockPerfRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<StaffPerformance, bool>>>(), null, null))
                .ReturnsAsync(performance);

          
            await _service.ReAssignStaffAsync(claimId, newStaffId, conversationId);


            Assert.Equal(ClaimStatus.Approved, claim.Status);
            Assert.Equal(newStaffId, conversation.ActiveStaffId);
            Assert.Equal(ConversationStatus.Pending, conversation.Status);
            Assert.Equal(1, performance.ReassignmentCount);

            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
            _mockTaskAction.Verify(t => t.CreateTaskActionAsync(It.IsAny<TaskActionRequest>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ReAssignStaffAsync_ClaimNotFound_ThrowsNotFoundException()
        {
           
            _mockClaimRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<Claim, bool>>>(), null, null))
                .ReturnsAsync((Claim)null);

        
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.ReAssignStaffAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task ReAssignStaffAsync_ConversationNoStaff_ThrowsBadRequestException()
        {
       
            var claim = new Claim { Id = Guid.NewGuid() };
            var conversation = new SupportConversation { Id = Guid.NewGuid(), ActiveStaffId = null }; // Không có staff

            _mockClaimRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<Claim, bool>>>(), null, null))
                .ReturnsAsync(claim);
            _mockConverRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<SupportConversation, bool>>>(), null, It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()))
                .ReturnsAsync(conversation);

           
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.ReAssignStaffAsync(claim.Id, Guid.NewGuid(), conversation.Id));
        }

        [Fact]
        public async Task ReAssignStaffAsync_NewStaffHighWorkload_SetsWarningStatus()
        {
 
            var claim = new Claim { Id = Guid.NewGuid() };
            var conversation = new SupportConversation { Id = Guid.NewGuid(), ActiveStaffId = Guid.NewGuid(), SupportTasks = new List<SupportTask>() };
            var newStaff = new Staff { Id = Guid.NewGuid() };

            _mockClaimRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<Claim, bool>>>(), null, null)).ReturnsAsync(claim);
            _mockConverRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<SupportConversation, bool>>>(), null, It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>())).ReturnsAsync(conversation);
            _mockStaffRepo.Setup(r => r.SingleOrDefaultAsync(It.IsAny<Expression<Func<Staff, bool>>>(), null, It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>())).ReturnsAsync(newStaff);

            // Giả lập nhân viên mới đang có 6 công việc (>= 5)
            _mockConverRepo.Setup(r => r.CountAsync(It.IsAny<Expression<Func<SupportConversation, bool>>>()))
                .ReturnsAsync(6);

            // Act
            await _service.ReAssignStaffAsync(claim.Id, newStaff.Id, conversation.Id);

            // Assert
            Assert.Equal(ConversationStatus.Warning, conversation.Status);
        }
    }
}
