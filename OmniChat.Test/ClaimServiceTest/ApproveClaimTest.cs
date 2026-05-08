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
    public class ApproveClaimTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ClaimService>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<ITaskActionService> _mockTaskAction;
        private readonly Mock<IHubContext<SupportConversationHub>> _mockHub;
        private readonly Mock<IGenericRepository<Claim>> _mockClaimRepo;

        private readonly ClaimService _service;

        public ApproveClaimTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ClaimService>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();
            _mockTaskAction = new Mock<ITaskActionService>();
            _mockHub = new Mock<IHubContext<SupportConversationHub>>();
            _mockClaimRepo = new Mock<IGenericRepository<Claim>>();

            _mockUow.Setup(u => u.GetRepository<Claim>()).Returns(_mockClaimRepo.Object);

            _mockUow.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task<ClaimDetailResponse>>>()))
                    .Returns((Func<Task<ClaimDetailResponse>> func) => func());

            _service = new ClaimService(
                _mockUow.Object,
                _mockLogger.Object,
                _mockMapper.Object,
                _mockAccessor.Object,
                _mockTaskAction.Object,
                _mockHub.Object);
        }

        [Fact]
        public async Task ApproveClaimAsync_ValidPendingClaim_ReturnsClaimDetailResponse()
        {
          
            var claimId = Guid.NewGuid();
            var existingClaim = new Claim
            {
                Id = claimId,
                Status = ClaimStatus.Pending,
                Description = "Test Approve"
            };

            var expectedResponse = new ClaimDetailResponse
            {
                Id = claimId,
                Status = ClaimStatus.Approved
            };

            // Mock tìm thấy Claim đang Pending
            _mockClaimRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Claim, bool>>>(),
                It.IsAny<Func<IQueryable<Claim>, IOrderedQueryable<Claim>>>(),
                It.IsAny<Func<IQueryable<Claim>, IIncludableQueryable<Claim, object>>>()
            )).ReturnsAsync(existingClaim);

            // Mock Map kết quả trả về
            _mockMapper.Setup(m => m.Map<ClaimDetailResponse>(existingClaim))
                       .Returns(expectedResponse);

            // Act
            var result = await _service.ApproveClaimAsync(claimId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(ClaimStatus.Approved, existingClaim.Status); // Kiểm tra xem Status đã đổi chưa
            _mockClaimRepo.Verify(r => r.Update(existingClaim), Times.Once);
            _mockUow.Verify(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task<ClaimDetailResponse>>>()), Times.Once);
        }

        [Fact]
        public async Task ApproveClaimAsync_ClaimNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var claimId = Guid.NewGuid();

            // Mock không tìm thấy Claim (trả về null)
            _mockClaimRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Claim, bool>>>(),
                It.IsAny<Func<IQueryable<Claim>, IOrderedQueryable<Claim>>>(),
                It.IsAny<Func<IQueryable<Claim>, IIncludableQueryable<Claim, object>>>()
            )).ReturnsAsync((Claim)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.ApproveClaimAsync(claimId));
        }

        [Fact]
        public async Task ApproveClaimAsync_ClaimNotPending_ThrowsBadRequestException()
        {
            var claimId = Guid.NewGuid();
            var notPendingClaim = new Claim
            {
                Id = claimId,
                Status = ClaimStatus.Approved 
            };

            _mockClaimRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Claim, bool>>>(),
                It.IsAny<Func<IQueryable<Claim>, IOrderedQueryable<Claim>>>(),
                It.IsAny<Func<IQueryable<Claim>, IIncludableQueryable<Claim, object>>>()
            )).ReturnsAsync(notPendingClaim);

           
            await Assert.ThrowsAsync<BadRequestException>(() => _service.ApproveClaimAsync(claimId));
        }
    }
}
