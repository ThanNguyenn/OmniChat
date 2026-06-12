using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Responses.Claim;
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
    public class RejectClaimTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ClaimService>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<ITaskActionService> _mockTaskAction;
        private readonly Mock<IHubContext<SupportConversationHub>> _mockHub;
        private readonly Mock<IGenericRepository<Claim>> _mockClaimRepo;
        private readonly Mock<ISupportConversationService> _mockConversationService;
        private readonly Mock<IMailService> _mockMailService;
        private readonly ClaimService _service;

        public RejectClaimTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ClaimService>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();
            _mockTaskAction = new Mock<ITaskActionService>();
            _mockHub = new Mock<IHubContext<SupportConversationHub>>();
            _mockClaimRepo = new Mock<IGenericRepository<Claim>>();
            _mockConversationService = new Mock<ISupportConversationService>();
            _mockMailService = new Mock<IMailService>();
            _mockUow.Setup(u => u.GetRepository<Claim>()).Returns(_mockClaimRepo.Object);


            _mockUow.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task<ClaimDetailResponse>>>()))
                    .Returns((Func<Task<ClaimDetailResponse>> func) => func());

            _service = new ClaimService(
                _mockUow.Object,
                _mockLogger.Object,
                _mockMapper.Object,
                _mockAccessor.Object,
                _mockTaskAction.Object, _mockMailService.Object,
_mockConversationService.Object,
                _mockHub.Object);
        }

        [Fact]
        public async Task RejectClaimAsync_ValidPendingClaim_ReturnsClaimDetailResponse()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var existingClaim = new Claim
            {
                Id = claimId,
                Status = ClaimStatus.Pending,
                Description = "Yêu cầu cần từ chối"
            };

            var expectedResponse = new ClaimDetailResponse
            {
                Id = claimId,
                Status = ClaimStatus.Rejected
            };

  
            _mockClaimRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Claim, bool>>>(),
                It.IsAny<Func<IQueryable<Claim>, IOrderedQueryable<Claim>>>(),
                It.IsAny<Func<IQueryable<Claim>, IIncludableQueryable<Claim, object>>>()
            )).ReturnsAsync(existingClaim);

            _mockMapper.Setup(m => m.Map<ClaimDetailResponse>(existingClaim))
                       .Returns(expectedResponse);

     
            var result = await _service.RejectClaimAsync(claimId);

 
            Assert.NotNull(result);
            Assert.Equal(ClaimStatus.Rejected, existingClaim.Status); 
            _mockClaimRepo.Verify(r => r.Update(existingClaim), Times.Once);
        }

        [Fact]
        public async Task RejectClaimAsync_ClaimNotFound_ThrowsNotFoundException()
        {
 
            var claimId = Guid.NewGuid();

            _mockClaimRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Claim, bool>>>(),
                It.IsAny<Func<IQueryable<Claim>, IOrderedQueryable<Claim>>>(),
                It.IsAny<Func<IQueryable<Claim>, IIncludableQueryable<Claim, object>>>()
            )).ReturnsAsync((Claim)null);

     
            await Assert.ThrowsAsync<NotFoundException>(() => _service.RejectClaimAsync(claimId));
        }

        [Fact]
        public async Task RejectClaimAsync_ClaimAlreadyProcessed_ThrowsBadRequestException()
        {
   
            var claimId = Guid.NewGuid();
            var processedClaim = new Claim
            {
                Id = claimId,
                Status = ClaimStatus.Approved 
            };

            _mockClaimRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Claim, bool>>>(),
                It.IsAny<Func<IQueryable<Claim>, IOrderedQueryable<Claim>>>(),
                It.IsAny<Func<IQueryable<Claim>, IIncludableQueryable<Claim, object>>>()
            )).ReturnsAsync(processedClaim);

            
            await Assert.ThrowsAsync<BadRequestException>(() => _service.RejectClaimAsync(claimId));
        }
    }
}
