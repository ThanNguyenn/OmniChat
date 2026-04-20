using OmniChat.Infrastructure.Dtos.Requests.Product;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatch;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
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
    Task<bool> UpdateProductImageAsync(Guid ProductId, UpdateProductImageRequest updateProductImageRequest);
    Task<bool> DeleteProductAsync (Guid ProductId);
    Task<PagingResponse<GetAllProductsResponse>> GetProductsAsync(PackagingType? PackagingType,ProductKind? ProductKind,double? VolumeMl,Guid? BrandId,string? search, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false);
    Task<GetProductResponse> GetProductByIdAsync(Guid ProductId);
    Task<IEnumerable<GetAllProductsCreateOrderResponse>> GetProductForCreateOrderByIdAsync(GetAllProductsCreateOrderQueryRequest? getAllProductsCreateOrderQueryRequest);
    Task AddStockAsync(IEnumerable<AddProductStockRequest> addProductStockRequests);

    Task<PagingResponse<GetProductBatchesResponse>> GetProductBatchesAsync(Guid productId, bool? isNewest, int pageNumber = 1, int pageSize = 20);

    public  Task<InventoryDashboardResponse> GetInventoryDashboardAsync();
} 
