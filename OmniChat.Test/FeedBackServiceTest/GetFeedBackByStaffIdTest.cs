using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.FeedBack;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace OmniChat.Test.FeedBackServiceTest
{
    public class GetFeedBackByStaffIdTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<FeedBack>> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly FeedBackService _service;

        public GetFeedBackByStaffIdTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<FeedBack>>();
            _mockMapper = new Mock<IMapper>();

            _mockUow.Setup(u => u.GetRepository<FeedBack>()).Returns(_mockRepo.Object);

            _service = new FeedBackService(
                _mockUow.Object,
                new Mock<ILogger<FeedBackService>>().Object,
                _mockMapper.Object,
                new Mock<IHttpContextAccessor>().Object);
        }


        private void SetupRepoMock(IQueryable<FeedBack> mockQueryable)
        {
            _mockRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<FeedBack, bool>>>(),       
                It.IsAny<Func<IQueryable<FeedBack>, IQueryable<FeedBack>>>(), 
                It.IsAny<bool>()                                   
            )).Returns(mockQueryable);
        }

        [Fact]
        public async Task GetFeedBackByStaffIdAsync_ValidStaffId_ReturnsPagingResponse()
        {
            
            var staffId = Guid.NewGuid();
            var feedbacks = new List<FeedBack>
            {
                new FeedBack { Id = Guid.NewGuid(), StaffId = staffId, Rating = 5, Content = "Good" },
                new FeedBack { Id = Guid.NewGuid(), StaffId = staffId, Rating = 4, Content = "Nice" },
                new FeedBack { Id = Guid.NewGuid(), StaffId = Guid.NewGuid(), Rating = 1, Content = "Bad" }
            };

            var mockQueryable = feedbacks.AsQueryable().BuildMock();
            SetupRepoMock(mockQueryable);

            _mockMapper.Setup(m => m.Map<IEnumerable<FeedBackResponse>>(It.IsAny<IEnumerable<FeedBack>>()))
                       .Returns(new List<FeedBackResponse> { new FeedBackResponse(), new FeedBackResponse() });

            var result = await _service.GetFeedBackByStaffIdAsync(staffId, 1, 10);

          
            Assert.NotNull(result);
            Assert.Equal(2, result.Meta.TotalItems);
        }

        [Fact]
        public async Task GetFeedBackByStaffIdAsync_NoFeedbackFound_ReturnsEmptyItems()
        {
           
            var staffId = Guid.NewGuid();
            var mockQueryable = new List<FeedBack>().AsQueryable().BuildMock();
            SetupRepoMock(mockQueryable);

            _mockMapper.Setup(m => m.Map<IEnumerable<FeedBackResponse>>(It.IsAny<IEnumerable<FeedBack>>()))
                       .Returns(new List<FeedBackResponse>());

           
            var result = await _service.GetFeedBackByStaffIdAsync(staffId, 1, 10);

            
            Assert.Empty(result.Items);
            Assert.Equal(0, result.Meta.TotalItems);
        }

        [Fact]
        public async Task GetFeedBackByStaffIdAsync_PagingLogic_CalculatesCorrectSkipTake()
        {
            
            var staffId = Guid.NewGuid();
            var feedbacks = new List<FeedBack>();
            for (int i = 0; i < 15; i++) feedbacks.Add(new FeedBack { StaffId = staffId, Rating = 5 });

            var mockQueryable = feedbacks.AsQueryable().BuildMock();
            SetupRepoMock(mockQueryable);

            
            var result = await _service.GetFeedBackByStaffIdAsync(staffId, 2, 10);

            
            Assert.Equal(15, result.Meta.TotalItems);
            Assert.Equal(2, result.Meta.TotalPages);
            Assert.Equal(2, result.Meta.CurrentPage);
        }
    }
}
