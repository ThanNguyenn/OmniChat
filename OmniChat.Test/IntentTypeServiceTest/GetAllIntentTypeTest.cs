using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.IntentType;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Query; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace OmniChat.Test.IntentTypeServiceTest
{
    public class GetAllIntentTypeTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<IntentType>> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly IntentTypeService _service;

        public GetAllIntentTypeTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<IntentType>>();
            _mockMapper = new Mock<IMapper>();

            _mockUow.Setup(u => u.GetRepository<IntentType>()).Returns(_mockRepo.Object);

            _service = new IntentTypeService(
                _mockUow.Object,
                new Mock<ILogger<IntentTypeService>>().Object,
                _mockMapper.Object,
                new Mock<IHttpContextAccessor>().Object);
        }

        [Fact]
        public async Task GetIntentTypesAsync_WhenCalled_ReturnsMappedIntentTypes()
        {
           
            var intentTypes = new List<IntentType>
            {
                new IntentType { Id = Guid.NewGuid(), TypeName = "Refund", IsActive = true, CreateDate = DateTime.Now },
                new IntentType { Id = Guid.NewGuid(), TypeName = "Support", IsActive = true, CreateDate = DateTime.Now.AddDays(-1) }
            };

            var expectedResponse = new List<GetsIntentTypeResponse>
            {
                new GetsIntentTypeResponse { TypeName = "Refund" },
                new GetsIntentTypeResponse { TypeName = "Support" } 
            };

      
            _mockRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<IntentType, bool>>>(),                                 
                It.IsAny<Func<IQueryable<IntentType>, IOrderedQueryable<IntentType>>>(),       
                It.IsAny<Func<IQueryable<IntentType>, IIncludableQueryable<IntentType, object>>>() 
            )).ReturnsAsync(intentTypes);

            _mockMapper.Setup(m => m.Map<IEnumerable<GetsIntentTypeResponse>>(intentTypes))
                       .Returns(expectedResponse);


            var result = await _service.GetIntentTypesAsync();


            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockRepo.Verify(r => r.GetListAsync(It.IsAny<Expression<Func<IntentType, bool>>>(), It.IsAny<Func<IQueryable<IntentType>, IOrderedQueryable<IntentType>>>(), null), Times.Once);
        }

        [Fact]
        public async Task GetIntentTypesAsync_NoDataFound_ReturnsEmptyList()
        {

            _mockRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<IntentType, bool>>>(),
                It.IsAny<Func<IQueryable<IntentType>, IOrderedQueryable<IntentType>>>(),
                null
            )).ReturnsAsync(new List<IntentType>());

            _mockMapper.Setup(m => m.Map<IEnumerable<GetsIntentTypeResponse>>(It.IsAny<IEnumerable<IntentType>>()))
                       .Returns(new List<GetsIntentTypeResponse>());


            var result = await _service.GetIntentTypesAsync();

            Assert.Empty(result);
        }
    }
}