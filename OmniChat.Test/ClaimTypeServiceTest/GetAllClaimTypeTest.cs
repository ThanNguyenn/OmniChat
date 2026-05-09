using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.ClaimType;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.ClaimTypeServiceTest
{
    public class GetAllClaimTypeTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<IGenericRepository<ClaimType>> _mockClaimTypeRepo;

        private readonly ClaimTypeService _service;

        public GetAllClaimTypeTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockMapper = new Mock<IMapper>();
            _mockAccessor = new Mock<IHttpContextAccessor>(); 
            _mockClaimTypeRepo = new Mock<IGenericRepository<ClaimType>>();

            _mockUow.Setup(u => u.GetRepository<ClaimType>()).Returns(_mockClaimTypeRepo.Object);


            _service = new ClaimTypeService(
                _mockUow.Object,
                new Mock<ILogger<ClaimTypeService>>().Object,
                _mockMapper.Object,
                _mockAccessor.Object); 
        }
        [Fact]
        public async Task GetAllTypeAsync_ActiveTypesExist_ReturnsList()
        {

            var claimTypes = new List<ClaimType>
            {
                new ClaimType { Id = Guid.NewGuid(), TypeName = "Change Task", IsActive = true }
            };

            var expectedResponses = new List<GetClaimTypeResponse>
            {
                new GetClaimTypeResponse { Id = claimTypes[0].Id, TypeName = "Change Task" }
            };

 
            _mockClaimTypeRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<ClaimType, bool>>>(), 
                It.IsAny<Func<IQueryable<ClaimType>, IOrderedQueryable<ClaimType>>>(), 
                It.IsAny<Func<IQueryable<ClaimType>, IIncludableQueryable<ClaimType, object>>>() 
            )).ReturnsAsync(claimTypes);

            _mockMapper.Setup(m => m.Map<IEnumerable<GetClaimTypeResponse>>(claimTypes))
                       .Returns(expectedResponses);

            var result = await _service.GetAllTypeAsync();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Change Task", result.First().TypeName);
        }

        [Fact]
        public async Task GetAllTypeAsync_NoActiveTypes_ReturnsEmptyList()
        {
          
            _mockClaimTypeRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<ClaimType, bool>>>(),
                null,
                null
            )).ReturnsAsync(new List<ClaimType>());

            _mockMapper.Setup(m => m.Map<IEnumerable<GetClaimTypeResponse>>(It.IsAny<IEnumerable<ClaimType>>()))
                       .Returns(new List<GetClaimTypeResponse>());

            var result = await _service.GetAllTypeAsync();

  
            Assert.Empty(result);
        }
    }
}
