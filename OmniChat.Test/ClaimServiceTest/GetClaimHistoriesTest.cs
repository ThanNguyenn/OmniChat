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
    public class GetClaimHistoriesTest
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

        public GetClaimHistoriesTest()
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
        public async Task GetClaimHistoryAsync_ShouldReturnCorrectPagingData()
        {

            var pageIndex = 1;
            var pageSize = 2;


            var claimsData = new List<Claim>
                {
                    new Claim { Id = Guid.NewGuid(), Status = ClaimStatus.Approved, SubmitDate = DateTime.Now.AddDays(-1) },
                    new Claim { Id = Guid.NewGuid(), Status = ClaimStatus.Rejected, SubmitDate = DateTime.Now },
                    new Claim { Id = Guid.NewGuid(), Status = ClaimStatus.Pending, SubmitDate = DateTime.Now.AddDays(-2) }
                };


            var mockDbSet = claimsData.AsQueryable().BuildMock();

            _mockClaimRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Claim, bool>>>(),
                It.IsAny<Func<IQueryable<Claim>, IQueryable<Claim>>>(),
                It.IsAny<bool>()
            )).Returns(mockDbSet);

            _mockMapper.Setup(m => m.Map<IEnumerable<ClaimDetailResponse>>(It.IsAny<IEnumerable<Claim>>()))
                       .Returns(new List<ClaimDetailResponse> { new ClaimDetailResponse(), new ClaimDetailResponse() });


            var result = await _service.GetClaimHistoryAsync(pageIndex, pageSize);


            Assert.NotNull(result);
            Assert.Equal(2, result.Meta.TotalItems);
            Assert.Equal(2, result.Items.Count());
            Assert.Equal(pageIndex, result.Meta.CurrentPage);
            Assert.Equal(1, result.Meta.TotalPages);
        }

        [Fact]
        public async Task GetClaimHistoryAsync_EmptyData_ShouldReturnEmptyPagingResponse()
        {
            // Arrange
            var claimsData = new List<Claim>().AsQueryable();
            var mockDbSet = claimsData.BuildMock();

            _mockClaimRepo.Setup(r => r.GetQueryable(
                  It.IsAny<Expression<Func<Claim, bool>>>(),
                  It.IsAny<Func<IQueryable<Claim>, IQueryable<Claim>>>(),
                  It.IsAny<bool>()
              )).Returns(mockDbSet);

            _mockMapper.Setup(m => m.Map<IEnumerable<ClaimDetailResponse>>(It.IsAny<IEnumerable<Claim>>()))
                       .Returns(new List<ClaimDetailResponse>());

            // Act
            var result = await _service.GetClaimHistoryAsync(1, 10);

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.Meta.TotalItems);
            Assert.Equal(0, result.Meta.TotalPages);
        }
    }
}
