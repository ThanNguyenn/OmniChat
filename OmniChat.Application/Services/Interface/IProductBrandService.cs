using OmniChat.Infrastructure.Dtos.Responses.Brand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IProductBrandService
{
    public Task<IEnumerable<GetAllBrandsResponse>> GetAllProductBrandsAsync();
}
