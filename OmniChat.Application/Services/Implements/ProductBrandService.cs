using AutoMapper;
using Microsoft.AspNetCore.Http;
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
}
