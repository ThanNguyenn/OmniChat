using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
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
    public class GetClaimsByStaffIdTest
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

        public GetClaimsByStaffIdTest()
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
        public async Task GetClaimsByStaffIdAsync_ClaimsExist_ReturnsPagingResponse()
        {
            // Arrange
            var staffId = Guid.NewGuid();
            var pageIndex = 1;
            var pageSize = 10;

            var claimsList = new List<Claim>
            {
                new Claim { Id = Guid.NewGuid(), StaffId = staffId, SubmitDate = DateTime.Now },
                new Claim { Id = Guid.NewGuid(), StaffId = staffId, SubmitDate = DateTime.Now.AddDays(-1) }
            };

            // Sử dụng MockQueryable để giả lập IQueryable hỗ trợ Async (ToListAsync, CountAsync)
            var mockQuery = claimsList.AsQueryable().BuildMock();

            _mockClaimRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Claim, bool>>>(),
                It.IsAny<Func<IQueryable<Claim>, IQueryable<Claim>>>(),
                It.IsAny<bool>()
            )).Returns(mockQuery);

            var expectedMappedItems = new List<ClaimDetailResponse>
            {
                new ClaimDetailResponse { Id = claimsList[0].Id },
                new ClaimDetailResponse { Id = claimsList[1].Id }
            };

            _mockMapper.Setup(m => m.Map<IEnumerable<ClaimDetailResponse>>(It.IsAny<List<Claim>>()))
                       .Returns(expectedMappedItems);

            // Act
            var result = await _service.GetClaimsByStaffIdAsync(staffId, pageIndex, pageSize);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Meta.TotalItems);
            Assert.Equal(pageIndex, result.Meta.CurrentPage);
            Assert.Equal(expectedMappedItems.Count, result.Items.Count());
            _mockClaimRepo.Verify(r => r.GetQueryable(null, null, false), Times.Once);
        }

        [Fact]
        public async Task GetClaimsByStaffIdAsync_NoClaimsFound_ThrowsNotFoundException()
        {
            // Arrange
            var staffId = Guid.NewGuid();

            // Danh sách trống
            var claimsList = new List<Claim>().AsQueryable().BuildMock();

            _mockClaimRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Claim, bool>>>(),
                It.IsAny<Func<IQueryable<Claim>, IQueryable<Claim>>>(),
                It.IsAny<bool>()
            )).Returns(claimsList);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetClaimsByStaffIdAsync(staffId, 1, 10));

            Assert.Equal("Không tìm thấy yêu cầu khiếu nại cho nhân viên này", exception.Message);
        }
    }
}
