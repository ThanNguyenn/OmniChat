using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.Provider;
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

namespace OmniChat.Test.ProviderServiceTest
{
    public class GetProviderPangingTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<Provider>> _mockRepo;
        private readonly ProviderService _service;

        public GetProviderPangingTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<Provider>>();

            _mockUow.Setup(u => u.GetRepository<Provider>()).Returns(_mockRepo.Object);

     
            _mockUow.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task<PagingResponse<GetAllProviderResponse>>>>()))
                .Returns<Func<Task<PagingResponse<GetAllProviderResponse>>>>(action => action());

            _service = new ProviderService(
                _mockUow.Object,
                new Mock<ILogger<ProviderService>>().Object,
                new Mock<AutoMapper.IMapper>().Object,
                new Mock<IHttpContextAccessor>().Object);
        }
        [Fact]
        public async Task GetAllProviderAsync_WhenCalled_ReturnsPagingList()
        {

            int pageNumber = 1;
            int pageSize = 10;
            string providerName = "Zalo";

            var mockData = new PagingResponse<GetAllProviderResponse>
            {
                Items = new List<GetAllProviderResponse>
                {
                    new GetAllProviderResponse { Id = Guid.NewGuid(), ProviderName = "Zalo", CreateDate = DateTime.UtcNow }
                },

                Meta = new PaginationMeta
                {
                    TotalItems = 1,
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalPages = 1
                }
            };

            _mockRepo.Setup(r => r.GetPagingListAsync<GetAllProviderResponse>(
                It.IsAny<Expression<Func<Provider, GetAllProviderResponse>>>(),
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Func<IQueryable<Provider>, IOrderedQueryable<Provider>>>(),
                It.IsAny<Func<IQueryable<Provider>, IIncludableQueryable<Provider, object>>>(), 
                It.IsAny<int>(),
                It.IsAny<int>()
            )).ReturnsAsync(mockData);


            var result = await _service.GetAllProviderAsync(pageNumber, pageSize, providerName);


            Assert.NotNull(result);
            Assert.Single(result.Items);
           
            Assert.Equal(1, result.Meta.TotalItems);
            Assert.Equal("Zalo", result.Items.First().ProviderName);

            _mockRepo.Verify(r => r.GetPagingListAsync<GetAllProviderResponse>(
                It.IsAny<Expression<Func<Provider, GetAllProviderResponse>>>(),
                It.IsAny<Expression<Func<Provider, bool>>>(),
                It.IsAny<Func<IQueryable<Provider>, IOrderedQueryable<Provider>>>(),
                It.IsAny<Func<IQueryable<Provider>, IIncludableQueryable<Provider, object>>>(),
                pageNumber,
                pageSize
            ), Times.Once);
        }

        [Fact]
        public async Task GetAllProviderAsync_WhenNoProviderFound_ReturnsEmptyItems()
        {
           
            var emptyData = new PagingResponse<GetAllProviderResponse>
            {
                Items = new List<GetAllProviderResponse>(),
                Meta = new PaginationMeta { TotalItems = 0 }
            };

            _mockRepo.Setup(r => r.GetPagingListAsync<GetAllProviderResponse>(
                It.IsAny<Expression<Func<Provider, GetAllProviderResponse>>>(),
                null,
                It.IsAny<Func<IQueryable<Provider>, IOrderedQueryable<Provider>>>(),
                null,
                It.IsAny<int>(),
                It.IsAny<int>()
            )).ReturnsAsync(emptyData);

      
            var result = await _service.GetAllProviderAsync(1, 20, null);

           
            Assert.Empty(result.Items);
           
            Assert.Equal(0, result.Meta.TotalItems);
        }
    }
}
