using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Brand;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;

namespace OmniChat.Application.Services.Implements;

public class ProductBrandService : BaseService<ProductBrandService>, IProductBrandService
{
    public ProductBrandService(IUnitOfWork<OmniChatDbContext> unitOfWork,
        ILogger<ProductBrandService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task<IEnumerable<GetAllBrandsResponse>> GetAllProductBrandsAsync()
    {
        return await _unitOfWork.GetRepository<Brand>().GetListAsync(
            predicate: p => p.IsActive != false,
            selector: e => _mapper.Map<GetAllBrandsResponse>(e)
        );
    }

    public async Task<int> GetTotalBrandsInStockAsync()
    {
        var productRepo = _unitOfWork.GetRepository<Product>();

        var totalBrands = await productRepo.GetQueryable()
            .Where(p => p.IsActive == true && p.Quantity > 0)
            .Select(p => p.BrandId)
            .Distinct()
            .CountAsync();

        return totalBrands;
    }

    public async Task<ProductBrandResponse> GetTotalProductByBrandIdAsync(Guid brandId)
    {
        var brandRepo = _unitOfWork.GetRepository<Brand>();
        var now = DateTime.UtcNow;

        var exitBrand = await brandRepo.SingleOrDefaultAsync(
            predicate: x => x.Id == brandId,
            include: x => x.Include(b => b.Products)
        );

        if (exitBrand == null) return null;

       
        var validProducts = exitBrand.Products
            .Where(p => p.CreateDate.HasValue &&
                        p.CreateDate.Value.AddDays(p.LifeSpan) > now)
            .ToList();

        var response = new ProductBrandResponse
        {
        
            TotalProduct = validProducts.Sum(p => p.Quantity),

            ProductKinds = validProducts
                .GroupBy(p => p.ProductKind)
                .Select(kindGroup => new ProductKindDetail
                {
                    KindName = kindGroup.Key.ToString(),
                    Volumes = kindGroup
                        .GroupBy(v => v.VolumeMl)
                        .OrderBy(v => v.Key) 
                        .Select(volumeGroup => new ProductVolumeDetail
                        {
                            Volume = volumeGroup.Key,
                            Quantity = volumeGroup.Sum(x => x.Quantity)
                        })
                        .ToList()
                })
                .ToList()
        };

        return response;
    }
}
