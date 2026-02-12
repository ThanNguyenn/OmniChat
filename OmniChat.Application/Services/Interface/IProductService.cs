using OmniChat.Infrastructure.Dtos.Requests.Product;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IProductService
{
    Task<bool> CreateProductAsync (CreateProductRequest createProductRequest);
    Task<bool> UpdateProductAsync (Guid ProductId, UpdateProductRequest updateProductRequest);
    Task<bool> DeleteProductAsync (Guid ProductId);

    Task<PagingResponse<GetAllProductsResponse>> GetProductsAsync(string? search, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false);

    Task<GetProductResponse> GetProductByIdAsync(Guid ProductId);

    Task AddStockAsync(IEnumerable<AddProductStockRequest> addProductStockRequests);
} 
