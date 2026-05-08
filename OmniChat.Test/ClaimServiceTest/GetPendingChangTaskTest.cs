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
    public class GetPendingChangTaskTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ClaimService>> _mockLogger;
        private readonly Mock<IGenericRepository<Claim>> _mockClaimRepo;

        private readonly ClaimService _service;

        public GetPendingChangTaskTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ClaimService>>();
            _mockClaimRepo = new Mock<IGenericRepository<Claim>>();

            _mockUow.Setup(u => u.GetRepository<Claim>()).Returns(_mockClaimRepo.Object);

            _service = new ClaimService(
                _mockUow.Object,
                _mockLogger.Object,
                _mockMapper.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<ITaskActionService>().Object,
                new Mock<IHubContext<SupportConversationHub>>().Object);
        }

        [Fact]
        public async Task GetPendingChangeTask_ShouldReturnPagingResponse_WhenDataExists()
        {

            var pageIndex = 1;
            var pageSize = 10;
            var changeTaskTypeId = Guid.Parse("abf8b2a1-0699-4c27-b241-11df7a75c12c");


            var claims = new List<Claim>
            {
                new Claim { Id = Guid.NewGuid(), Status = ClaimStatus.Pending, ClaimTypeId = changeTaskTypeId, SubmitDate = DateTime.UtcNow },
                new Claim { Id = Guid.NewGuid(), Status = ClaimStatus.Pending, ClaimTypeId = changeTaskTypeId, SubmitDate = DateTime.UtcNow.AddMinutes(-5) }
            }.AsQueryable().BuildMock();

            _mockClaimRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Claim, bool>>>(),
                It.IsAny<Func<IQueryable<Claim>, IQueryable<Claim>>>(),
                It.IsAny<bool>()
            )).Returns(claims);

            var mappedItems = new List<ClaimDetailResponse>
            {
                new ClaimDetailResponse { Id = Guid.NewGuid() },
                new ClaimDetailResponse { Id = Guid.NewGuid() }
            };

            _mockMapper.Setup(m => m.Map<IEnumerable<ClaimDetailResponse>>(It.IsAny<IEnumerable<Claim>>()))
                       .Returns(mappedItems);


            var result = await _service.GetPendingChangeTask(pageIndex, pageSize);


            Assert.NotNull(result);
            Assert.Equal(2, result.Meta.TotalItems);
            Assert.Equal(pageIndex, result.Meta.CurrentPage);
            Assert.Equal(mappedItems.Count, result.Items.Count());

            _mockClaimRepo.Verify(r => r.GetQueryable(null, null, false), Times.Once);
        }

        [Fact]
        public async Task GetPendingChangeTask_ShouldReturnEmpty_WhenNoDataMatches()
        {

            var claims = new List<Claim>().AsQueryable().BuildMock();

            _mockClaimRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Claim, bool>>>(),
                It.IsAny<Func<IQueryable<Claim>, IQueryable<Claim>>>(),
                It.IsAny<bool>()
            )).Returns(claims);

            _mockMapper.Setup(m => m.Map<IEnumerable<ClaimDetailResponse>>(It.IsAny<IEnumerable<Claim>>()))
                       .Returns(new List<ClaimDetailResponse>());


            var result = await _service.GetPendingChangeTask(1, 10);


            Assert.Empty(result.Items);
            Assert.Equal(0, result.Meta.TotalItems);
            Assert.Equal(0, result.Meta.TotalPages);
        }
    }
}
