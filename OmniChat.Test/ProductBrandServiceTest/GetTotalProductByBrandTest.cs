using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
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
    public class GetTotalProductByBrandTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<Brand>> _mockRepo;
        private readonly ProductBrandService _service;

        public GetTotalProductByBrandTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<Brand>>();

            _mockUow.Setup(u => u.GetRepository<Brand>()).Returns(_mockRepo.Object);

            _service = new ProductBrandService(
                _mockUow.Object,
                new Mock<ILogger<ProductBrandService>>().Object,
                new Mock<AutoMapper.IMapper>().Object,
                new Mock<IHttpContextAccessor>().Object);
        }
        [Fact]
        public async Task GetTotalProductByBrandIdAsync_ValidBrand_ReturnsGroupedProductStats()
        {

            var brandId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var brand = new Brand
            {
                Id = brandId,
                Products = new List<Product>
                {

                    new Product { ProductKind = ProductKind.Sugar, VolumeMl = 180, Quantity = 10, CreateDate = now, LifeSpan = 30 },
                    new Product { ProductKind = ProductKind.Sugar, VolumeMl = 180, Quantity = 20, CreateDate = now, LifeSpan = 30 },

                    new Product { ProductKind = ProductKind.Sugar, VolumeMl = 110, Quantity = 50, CreateDate = now.AddDays(-10), LifeSpan = 1 },

                    new Product { ProductKind = ProductKind.Yogurt, VolumeMl = 100, Quantity = 15, CreateDate = now, LifeSpan = 30 }
                }
            };


            _mockRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Brand, bool>>>(),                               
                It.IsAny<Func<IQueryable<Brand>, IOrderedQueryable<Brand>>>(),           
                It.IsAny<Func<IQueryable<Brand>, IIncludableQueryable<Brand, object>>>()  
            )).ReturnsAsync(brand);

            var result = await _service.GetTotalProductByBrandIdAsync(brandId);


            Assert.NotNull(result);

            Assert.Equal(45, result.TotalProduct);

            var sugarKind = result.ProductKinds.FirstOrDefault(k => k.KindName == "Sugar");
            Assert.NotNull(sugarKind);
            Assert.Equal(30, sugarKind.Volumes.First(v => v.Volume == 180).Quantity);


            Assert.Contains(result.ProductKinds, k => k.KindName == "Yogurt");
        }

        [Fact]
        public async Task GetTotalProductByBrandIdAsync_BrandNotFound_ReturnsNull()
        {

            var brandId = Guid.NewGuid();

            _mockRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Brand, bool>>>(),
                It.IsAny<Func<IQueryable<Brand>, IOrderedQueryable<Brand>>>(),
                It.IsAny<Func<IQueryable<Brand>, IIncludableQueryable<Brand, object>>>()
            )).ReturnsAsync((Brand)null);


            var result = await _service.GetTotalProductByBrandIdAsync(brandId);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTotalProductByBrandIdAsync_AllProductsExpired_ReturnsZeroTotal()
        {

            var brandId = Guid.NewGuid();
            var brand = new Brand
            {
                Id = brandId,
                Products = new List<Product>
                {
                  
                    new Product { Quantity = 100, CreateDate = DateTime.UtcNow.AddDays(-100), LifeSpan = 10 }
                }
            };

            _mockRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Brand, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<Brand>, IIncludableQueryable<Brand, object>>>()
            )).ReturnsAsync(brand);

          
            var result = await _service.GetTotalProductByBrandIdAsync(brandId);

          
            Assert.Equal(0, result.TotalProduct);
            Assert.Empty(result.ProductKinds);
        }
    }
}
