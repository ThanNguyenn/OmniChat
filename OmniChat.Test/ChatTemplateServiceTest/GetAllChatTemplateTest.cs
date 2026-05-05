using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.ChatTemplate;
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

namespace OmniChat.Test.ChatTemplateServiceTest
{
    public class GetAllChatTemplateTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ChatTemplateService>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<IGenericRepository<ChatTemplate>> _mockRepo;
        private readonly ChatTemplateService _service;

        public GetAllChatTemplateTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ChatTemplateService>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();
            _mockRepo = new Mock<IGenericRepository<ChatTemplate>>();

            _mockUow.Setup(u => u.GetRepository<ChatTemplate>()).Returns(_mockRepo.Object);

            _service = new ChatTemplateService(
                _mockUow.Object,
                _mockLogger.Object,
                _mockMapper.Object,
                _mockAccessor.Object
            );
        }

        [Fact]
        public async Task GetAllChatTemplateAsync_ShouldReturnEmpty_WhenNoDataExists()
        {
            
            int page = 1, size = 10;
            var emptyPagingResponse = new PagingResponse<ChatTemplateResponse>
            {
                Items = new List<ChatTemplateResponse>(),
                Meta = new PaginationMeta { TotalItems = 0, TotalPages = 0, CurrentPage = 1, PageSize = 10 }
            };

          
            _mockRepo.Setup(r => r.GetPagingListAsync<ChatTemplateResponse>(
                It.IsAny<Expression<Func<ChatTemplate, ChatTemplateResponse>>>(), 
                It.IsAny<Expression<Func<ChatTemplate, bool>>>(),                
                It.IsAny<Func<IQueryable<ChatTemplate>, IOrderedQueryable<ChatTemplate>>>(), 
                It.IsAny<Func<IQueryable<ChatTemplate>, IIncludableQueryable<ChatTemplate, object>>>(),
                page, 
                size  
            )).ReturnsAsync(emptyPagingResponse);

            
            var result = await _service.GetAllChatTemplateAsync(page, size, null);

            result.Items.Should().BeEmpty();
            result.Meta.TotalItems.Should().Be(0);
        }

        [Fact]
        public async Task GetAllChatTemplateAsync_ShouldReturnData_WhenSearchMatches()
        {
            // Arrange
            string search = "O0";
            var mockData = new List<ChatTemplateResponse>
        {
            new ChatTemplateResponse { Id = Guid.NewGuid(), Code = "O01", Content = "Đơn đang giao" }
        };

            var pagingResponse = new PagingResponse<ChatTemplateResponse>
            {
                Items = mockData,
                Meta = new PaginationMeta { TotalItems = 1, TotalPages = 1, CurrentPage = 1, PageSize = 10 }
            };

   
            _mockRepo.Setup(r => r.GetPagingListAsync<ChatTemplateResponse>(
                It.IsAny<Expression<Func<ChatTemplate, ChatTemplateResponse>>>(), 
                It.IsAny<Expression<Func<ChatTemplate, bool>>>(),                
                It.IsAny<Func<IQueryable<ChatTemplate>, IOrderedQueryable<ChatTemplate>>>(), 
                It.IsAny<Func<IQueryable<ChatTemplate>, IIncludableQueryable<ChatTemplate, object>>>(), 
                1, 
                10 
            )).ReturnsAsync(pagingResponse);

            
            var result = await _service.GetAllChatTemplateAsync(1, 10, search);

           
            result.Items.Should().NotBeEmpty();
            result.Meta.TotalItems.Should().Be(1);
        }

        [Fact]
        public async Task GetAllChatTemplateAsync_ShouldReturnAllData_WhenSearchIsNull()
        {
          
            var mockData = new List<ChatTemplateResponse>
        {
            new ChatTemplateResponse { Code = "H01" },
            new ChatTemplateResponse { Code = "H11" }
        };

            var pagingResponse = new PagingResponse<ChatTemplateResponse>
            {
                Items = mockData,
                Meta = new PaginationMeta { TotalItems = 2, TotalPages = 1, CurrentPage = 1, PageSize = 10 }
            };

            
            _mockRepo.Setup(r => r.GetPagingListAsync<ChatTemplateResponse>(
                It.IsAny<Expression<Func<ChatTemplate, ChatTemplateResponse>>>(), 
                It.IsAny<Expression<Func<ChatTemplate, bool>>>(),              
                It.IsAny<Func<IQueryable<ChatTemplate>, IOrderedQueryable<ChatTemplate>>>(), 
                It.IsAny<Func<IQueryable<ChatTemplate>, IIncludableQueryable<ChatTemplate, object>>>(),
                1, 
                10 
            )).ReturnsAsync(pagingResponse);

            // Act
            var result = await _service.GetAllChatTemplateAsync(1, 10, null);

        
            result.Items.Should().NotBeEmpty();
            result.Meta.TotalItems.Should().Be(2);
        }
    }
}
