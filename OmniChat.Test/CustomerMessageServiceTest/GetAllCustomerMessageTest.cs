using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.CustomerMessage;
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

namespace OmniChat.Test.CustomerMessageServiceTest
{
    public class GetAllCustomerMessageTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<CustomerMessage>> _mockMsgRepo;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly CustomerMessageService _service;

        public GetAllCustomerMessageTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockMsgRepo = new Mock<IGenericRepository<CustomerMessage>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();

            _mockUow.Setup(u => u.GetRepository<CustomerMessage>()).Returns(_mockMsgRepo.Object);

            _service = new CustomerMessageService(
                _mockUow.Object,
                new Mock<ILogger<CustomerMessageService>>().Object,
                new Mock<AutoMapper.IMapper>().Object,
                _mockAccessor.Object);
        }

        [Fact]
        public async Task GetAllCustomerMessageByCustomerIdAsync_WithCustomerId_ReturnsPagingResponse()
        {

            var customerId = Guid.NewGuid();
            int pageNumber = 1;
            int pageSize = 20;

            var expectedData = new PagingResponse<GetAllCustomerMessageResponse>
            {
                Items = new List<GetAllCustomerMessageResponse>
                {
                    new GetAllCustomerMessageResponse { Id = Guid.NewGuid(), Content = "Hello", CustomerId = customerId }
                },
                Meta = new PaginationMeta { TotalItems = 1, CurrentPage = 1, PageSize = 20 }
            };

 
            _mockMsgRepo.Setup(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<CustomerMessage, GetAllCustomerMessageResponse>>>(),
                It.IsAny<Expression<Func<CustomerMessage, bool>>>(),                        
                It.IsAny<Func<IQueryable<CustomerMessage>, IOrderedQueryable<CustomerMessage>>>(), 
                null,
                pageNumber,
                pageSize
            )).ReturnsAsync(expectedData);


            var result = await _service.GetAllCustomerMessageByCustomerIdAsync(pageNumber, pageSize, customerId);


            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("Hello", result.Items.First().Content);
            Assert.Equal(customerId, result.Items.First().CustomerId);

            _mockMsgRepo.Verify(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<CustomerMessage, GetAllCustomerMessageResponse>>>(),
                It.IsNotNull<Expression<Func<CustomerMessage, bool>>>(), 
                It.IsAny<Func<IQueryable<CustomerMessage>, IOrderedQueryable<CustomerMessage>>>(),
                null,
                pageNumber,
                pageSize), Times.Once);
        }

        [Fact]
        public async Task GetAllCustomerMessageByCustomerIdAsync_NullCustomerId_ReturnsAllMessages()
        {
           
            var expectedData = new PagingResponse<GetAllCustomerMessageResponse>
            {
                Items = new List<GetAllCustomerMessageResponse> { new GetAllCustomerMessageResponse() },
                Meta = new PaginationMeta { TotalItems = 1 }
            };

            _mockMsgRepo.Setup(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<CustomerMessage, GetAllCustomerMessageResponse>>>(),
                null, 
                It.IsAny<Func<IQueryable<CustomerMessage>, IOrderedQueryable<CustomerMessage>>>(),
                null,
                1,
                20
            )).ReturnsAsync(expectedData);

         
            var result = await _service.GetAllCustomerMessageByCustomerIdAsync(1, 20, null);

        
            Assert.NotNull(result);
            _mockMsgRepo.Verify(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<CustomerMessage, GetAllCustomerMessageResponse>>>(),
                null,
                It.IsAny<Func<IQueryable<CustomerMessage>, IOrderedQueryable<CustomerMessage>>>(),
                null,
                1,
                20), Times.Once);
        }

        [Fact]
        public async Task GetAllCustomerMessageByCustomerIdAsync_EmptyResult_ReturnsEmptyPaging()
        {
   
            var emptyResponse = new PagingResponse<GetAllCustomerMessageResponse>
            {
                Items = new List<GetAllCustomerMessageResponse>(),
                Meta = new PaginationMeta { TotalItems = 0 }
            };

            _mockMsgRepo.Setup(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<CustomerMessage, GetAllCustomerMessageResponse>>>(),
                It.IsAny<Expression<Func<CustomerMessage, bool>>>(),
                It.IsAny<Func<IQueryable<CustomerMessage>, IOrderedQueryable<CustomerMessage>>>(),
                null,
                It.IsAny<int>(),
                It.IsAny<int>()
            )).ReturnsAsync(emptyResponse);

            
            var result = await _service.GetAllCustomerMessageByCustomerIdAsync(1, 20, Guid.NewGuid());

     
            Assert.Empty(result.Items);
            Assert.Equal(0, result.Meta.TotalItems);
        }
    }
}
