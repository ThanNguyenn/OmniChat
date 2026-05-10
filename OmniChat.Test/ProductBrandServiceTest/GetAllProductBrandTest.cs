using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.Brand;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.ProductBrandServiceTest
{
    public class GetAllProductBrandTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<Brand>> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ProductBrandService _service;

        public GetAllProductBrandTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<Brand>>();
            _mockMapper = new Mock<IMapper>();

            _mockUow.Setup(u => u.GetRepository<Brand>()).Returns(_mockRepo.Object);

            _service = new ProductBrandService(
                _mockUow.Object,
                new Mock<ILogger<ProductBrandService>>().Object,
                _mockMapper.Object,
                new Mock<IHttpContextAccessor>().Object);
        }

        [Fact]
        public async Task GetAllProductBrandsAsync_WhenCalled_ReturnsListOfBrands()
        {

            var brandsResponse = new List<GetAllBrandsResponse>
    {
        new GetAllBrandsResponse { Id = Guid.NewGuid(), Name = "Brand A" },
        new GetAllBrandsResponse { Id = Guid.NewGuid(), Name = "Brand B" }
    };


            _mockRepo.Setup(r => r.GetListAsync<GetAllBrandsResponse>(
                It.IsAny<Expression<Func<Brand, GetAllBrandsResponse>>>(),                 
                It.IsAny<Expression<Func<Brand, bool>>>(),                                
                It.IsAny<Func<IQueryable<Brand>, IOrderedQueryable<Brand>>>(),             
                It.IsAny<Func<IQueryable<Brand>, IIncludableQueryable<Brand, object>>>()   
            )).ReturnsAsync((ICollection<GetAllBrandsResponse>)brandsResponse);


            var result = await _service.GetAllProductBrandsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Brand A", result.First().Name);

            
            _mockRepo.Verify(r => r.GetListAsync<GetAllBrandsResponse>(
                It.IsAny<Expression<Func<Brand, GetAllBrandsResponse>>>(),
                It.IsAny<Expression<Func<Brand, bool>>>(),
                It.IsAny<Func<IQueryable<Brand>, IOrderedQueryable<Brand>>>(),
                It.IsAny<Func<IQueryable<Brand>, IIncludableQueryable<Brand, object>>>()
            ), Times.Once);
        }

        [Fact]
        public async Task GetAllProductBrandsAsync_NoBrandsExist_ReturnsEmptyList()
        {
            
            _mockRepo.Setup(r => r.GetListAsync<GetAllBrandsResponse>(
                It.IsAny<Expression<Func<Brand, GetAllBrandsResponse>>>(),
                It.IsAny<Expression<Func<Brand, bool>>>(),
                It.IsAny<Func<IQueryable<Brand>, IOrderedQueryable<Brand>>>(),
                It.IsAny<Func<IQueryable<Brand>, IIncludableQueryable<Brand, object>>>()
            )).ReturnsAsync(new List<GetAllBrandsResponse>());

            var result = await _service.GetAllProductBrandsAsync();

            
            Assert.Empty(result);
        }
    }
}
