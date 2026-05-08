using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
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
    public class UpdateClaimTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ClaimService>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<ITaskActionService> _mockTaskAction;
        private readonly Mock<IHubContext<SupportConversationHub>> _mockHub;
        private readonly Mock<IGenericRepository<Claim>> _mockClaimRepo;

        private readonly ClaimService _service;

        public UpdateClaimTest() {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ClaimService>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();
            _mockTaskAction = new Mock<ITaskActionService>();
            _mockHub = new Mock<IHubContext<SupportConversationHub>>();
            _mockClaimRepo = new Mock<IGenericRepository<Claim>>();

            _mockUow.Setup(u => u.GetRepository<Claim>()).Returns(_mockClaimRepo.Object);

         
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
        public async Task UpdateClaimInforAsync_ValidRequest_ReturnsTrue()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var request = new UpdateClaimRequest
            {
                ClaimTypeId = Guid.NewGuid(),
                Description = "Mô tả mới hợp lệ",
                Reason = "Lý do thay đổi"
            };

            var existingClaim = new Claim
            {
                Id = claimId,
                Status = ClaimStatus.Pending,
                Description = "Mô tả cũ"
            };

            // SETUP TRANSACTION
            _mockUow.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
                    .Returns((Func<Task<bool>> func) => func());

          
            _mockClaimRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Claim, bool>>>(),                            
                It.IsAny<Func<IQueryable<Claim>, IOrderedQueryable<Claim>>>(),       
                It.IsAny<Func<IQueryable<Claim>, IIncludableQueryable<Claim, object>>>() 
            )).ReturnsAsync(existingClaim);

            // SETUP MAPPER
            _mockMapper.Setup(m => m.Map(request, existingClaim)).Returns(existingClaim);

            // Act
            var result = await _service.UpdateClaimInforAsync(claimId, request);

            // Assert
            Assert.True(result);
            _mockClaimRepo.Verify(r => r.Update(existingClaim), Times.Once);
        }

        [Fact]
        public async Task UpdateClaimInforAsync_RequestNull_ThrowsBadRequestException()
        {
            
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.UpdateClaimInforAsync(Guid.NewGuid(), null));
        }

        [Fact]
        public async Task UpdateClaimInforAsync_ClaimNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var claimId = Guid.NewGuid();
            var request = new UpdateClaimRequest { ClaimTypeId = Guid.NewGuid() };

            _mockUow.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
                    .Returns((Func<Task<bool>> func) => func());

            // Mock trả về null
            _mockClaimRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Claim, bool>>>(),
                It.IsAny<Func<IQueryable<Claim>, IOrderedQueryable<Claim>>>(),
                It.IsAny<Func<IQueryable<Claim>, IIncludableQueryable<Claim, object>>>()
            )).ReturnsAsync((Claim)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdateClaimInforAsync(claimId, request));
        }
    }
}
