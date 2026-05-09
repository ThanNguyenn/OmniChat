using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.WarningConversation;
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

namespace OmniChat.Test.ConversationWarningServiceTest
{
    public class GetAllConversationWarningTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<ConversationWarning>> _mockWarningRepo;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly ConversationWarningService _service;

        public GetAllConversationWarningTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockWarningRepo = new Mock<IGenericRepository<ConversationWarning>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();

            _mockUow.Setup(u => u.GetRepository<ConversationWarning>()).Returns(_mockWarningRepo.Object);

            _service = new ConversationWarningService(
                _mockUow.Object,
                new Mock<ILogger<ConversationWarningService>>().Object,
                new Mock<AutoMapper.IMapper>().Object,
                _mockAccessor.Object);
        }

        [Fact]
        public async Task GetAllWarningsAsync_CorrectArguments_ReturnsData()
        {

            int pageNumber = 1;
            int pageSize = 10;

            var expectedData = new PagingResponse<WarningDetailRepsone>
            {
                Items = new List<WarningDetailRepsone>
                {
                    new WarningDetailRepsone { Id = Guid.NewGuid(), CustomerName = "Test Customer" }
                },
                Meta = new PaginationMeta { TotalItems = 1, CurrentPage = 1, PageSize = 10 }
            };

 
            _mockWarningRepo.Setup(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<ConversationWarning, WarningDetailRepsone>>>(), 
                It.IsAny<Expression<Func<ConversationWarning, bool>>>(),                
                It.IsAny<Func<IQueryable<ConversationWarning>, IOrderedQueryable<ConversationWarning>>>(),
                It.IsAny<Func<IQueryable<ConversationWarning>, IIncludableQueryable<ConversationWarning, object>>>(), 
                pageNumber,
                pageSize   
            )).ReturnsAsync(expectedData);


            var result = await _service.GetAllWarningsAsync(pageNumber, pageSize, null);

            Assert.NotNull(result);
            Assert.Equal(1, result.Meta.TotalItems);
            _mockWarningRepo.Verify(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<ConversationWarning, WarningDetailRepsone>>>(),
                It.IsAny<Expression<Func<ConversationWarning, bool>>>(),
                It.IsAny<Func<IQueryable<ConversationWarning>, IOrderedQueryable<ConversationWarning>>>(),
                It.IsAny<Func<IQueryable<ConversationWarning>, IIncludableQueryable<ConversationWarning, object>>>(),
                pageNumber,
                pageSize), Times.Once);
        }

        [Fact]
        public async Task GetAllWarningsAsync_WithFilter_PassesCorrectPredicate()
        {

            bool isReviewed = true;

            _mockWarningRepo.Setup(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<ConversationWarning, WarningDetailRepsone>>>(),
                It.IsAny<Expression<Func<ConversationWarning, bool>>>(),
                It.IsAny<Func<IQueryable<ConversationWarning>, IOrderedQueryable<ConversationWarning>>>(),
                It.IsAny<Func<IQueryable<ConversationWarning>, IIncludableQueryable<ConversationWarning, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            )).ReturnsAsync(new PagingResponse<WarningDetailRepsone> { Items = new List<WarningDetailRepsone>() });

            await _service.GetAllWarningsAsync(1, 10, isReviewed);

           
            _mockWarningRepo.Verify(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<ConversationWarning, WarningDetailRepsone>>>(),
                It.IsNotNull<Expression<Func<ConversationWarning, bool>>>(),
                It.IsAny<Func<IQueryable<ConversationWarning>, IOrderedQueryable<ConversationWarning>>>(),
                It.IsAny<Func<IQueryable<ConversationWarning>, IIncludableQueryable<ConversationWarning, object>>>(),
                1,
                10), Times.Once);
        }
    }
}
