using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.SupportTask;
using OmniChat.Infrastructure.Dtos.Responses.SupportTask;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.StaffServiceTest
{
    public class StaffTaskTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<SupportTask>> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly StaffService _service;

        public StaffTaskTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<SupportTask>>();
            _mockMapper = new Mock<IMapper>();

            _mockUow.Setup(u => u.GetRepository<SupportTask>()).Returns(_mockRepo.Object);

            _service = new StaffService(
                _mockUow.Object,
                new Mock<ILogger<StaffService>>().Object,
                _mockMapper.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IR2StorageService>().Object);
        }

        [Fact]
        public async Task GetStaffTasksAsync_ValidRequest_ReturnsPagingResponse()
        {
            var staffId = Guid.NewGuid();
            var request = new StaffTaskFilterRequest
            {
                Page = 1,
                PageSize = 10,
                TaskName = "Support"
            };

            var mockTasks = new List<SupportTask>
            {
                new SupportTask { Id = Guid.NewGuid(), CurrentAssignedStaffId = staffId, CreatedAt = DateTime.UtcNow }
            };

            var mockRepoResult = new PagingResponse<SupportTask>
            {
                Items = mockTasks,
                Meta = new PaginationMeta { TotalItems = 1, TotalPages = 1 }
            };

            var mappedTasks = new List<StaffSupportTaskResponse>
            {
                new StaffSupportTaskResponse { Id = mockTasks[0].Id }
            };

            _mockRepo.Setup(r => r.GetPagingListAsync(
    It.IsAny<Expression<Func<SupportTask, bool>>>(),               
    It.IsAny<Func<IQueryable<SupportTask>, IOrderedQueryable<SupportTask>>>(), 
    It.IsAny<Func<IQueryable<SupportTask>, IIncludableQueryable<SupportTask, object>>>(), 
    It.IsAny<int>(),
    It.IsAny<int>()
)).ReturnsAsync(mockRepoResult);

            _mockMapper.Setup(m => m.Map<List<StaffSupportTaskResponse>>(It.IsAny<IEnumerable<SupportTask>>()))
                       .Returns(mappedTasks);

            var result = await _service.GetStaffTasksAsync(staffId, request);

            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(1, result.Meta.TotalItems);
            Assert.Equal(request.Page, result.Meta.CurrentPage);

            _mockRepo.Verify(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<SupportTask, bool>>>(),
                It.IsAny<Func<IQueryable<SupportTask>, IOrderedQueryable<SupportTask>>>(),
                It.IsAny<Func<IQueryable<SupportTask>, IIncludableQueryable<SupportTask, object>>>(),
                1,
                10
            ), Times.Once);
        }

        [Fact]
        public async Task GetStaffTasksAsync_RequestNull_UsesDefaultPagination()
        {
            var staffId = Guid.NewGuid();

            var mockRepoResult = new PagingResponse<SupportTask>
            {
                Items = new List<SupportTask>(),
                Meta = new PaginationMeta { TotalItems = 0, TotalPages = 0 }
            };

            _mockRepo.Setup(r => r.GetPagingListAsync(
             It.IsAny<Expression<Func<SupportTask, bool>>>(),                
             It.IsAny<Func<IQueryable<SupportTask>, IOrderedQueryable<SupportTask>>>(), 
             It.IsAny<Func<IQueryable<SupportTask>, IIncludableQueryable<SupportTask, object>>>(),
             It.IsAny<int>(),                                        
             It.IsAny<int>()                                            
         )).ReturnsAsync(mockRepoResult);

            _mockMapper.Setup(m => m.Map<List<StaffSupportTaskResponse>>(It.IsAny<IEnumerable<SupportTask>>()))
                       .Returns(new List<StaffSupportTaskResponse>());

          
            var result = await _service.GetStaffTasksAsync(staffId, null);

            Assert.Equal(1, result.Meta.CurrentPage);
            Assert.Equal(10, result.Meta.PageSize);
        }
    }
}
