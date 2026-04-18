using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Product;
using OmniChat.Infrastructure.Dtos.Requests.ProductBatch;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatch;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class ProductService : BaseService<ProductService>, IProductService
{
    private readonly IR2StorageService _storageService;
    public ProductService(
        IUnitOfWork<OmniChatDbContext> unitOfWork,
        ILogger<ProductService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IR2StorageService storageService)
    : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _storageService = storageService;
    }

    public async Task<bool> CreateProductAsync(CreateProductRequest createProductRequest)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();

        Product newProduct = null;

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            newProduct = _mapper.Map<Product>(createProductRequest);

            newProduct.ImageUrl = "https://pub-28eb3560d5b74d478da589a1c3dd7e34.r2.dev/products/default_product.webp";

            await productRepo.InsertAsync(newProduct);
        });

        if (createProductRequest.Image != null && createProductRequest.Image.Length > 0)
        {
            await _storageService.UploadUpdatedImageAsync(
                createProductRequest.Image.OpenReadStream(),
                createProductRequest.Image.FileName,
                "products",
                newProduct.Id
            );
        }
        return true;
    }

    public async Task<bool> DeleteProductAsync(Guid ProductId)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();
        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var existingProduct = await productRepo.SingleOrDefaultAsync(predicate: p => p.Id == ProductId);
            if (existingProduct == null)
            {
                throw new NotFoundException("Product not found");
            }
            existingProduct.IsActive = false;
            productRepo.Update(existingProduct);
        });
        return true;
    }

    public async Task<bool> UpdateProductAsync(Guid ProductId, UpdateProductRequest updateProductRequest)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();
        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var existingProduct = await productRepo.SingleOrDefaultAsync(predicate: p => p.Id == ProductId);
            if (existingProduct == null)
            {
                throw new NotFoundException("Product not found");
            }
            _mapper.Map(updateProductRequest, existingProduct);
            productRepo.Update(existingProduct);
        });
        return true;
    }

    public async Task<bool> UpdateProductImageAsync(Guid productId, UpdateProductImageRequest request)
    {
        if (request?.Image == null || request.Image.Length == 0)
            throw new BusinessException("Invalid image file.");

        var stream = request.Image.OpenReadStream();
        var fileName = request.Image.FileName;

        var result = await _storageService.UploadUpdatedImageAsync(
            stream,
            fileName,
            "products",
            productId
        );

        return result;
    }

    public async Task<PagingResponse<GetAllProductsResponse>> GetProductsAsync(string? search, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();
        var response = await productRepo.GetPagingListAsync<GetAllProductsResponse>(
                predicate: p => p.IsActive != false && (string.IsNullOrEmpty(search) || p.Name.Contains(search) || p.Code.Contains(search)),
                orderBy: q => OrderBy(q, sortBy, descending),
                selector: e => _mapper.Map<GetAllProductsResponse>(e),
                include: q => q.Include(p => p.Brand),
                page: pageNumber,
                size: pageSize
                );
        return response;
    }

    public async Task<IEnumerable<GetAllProductsCreateOrderResponse>> GetProductForCreateOrderByIdAsync(GetAllProductsCreateOrderQueryRequest? getAllProductsCreateOrderQueryRequest)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();

        var request = getAllProductsCreateOrderQueryRequest ?? new GetAllProductsCreateOrderQueryRequest();

        var response = await productRepo.GetListAsync<GetAllProductsCreateOrderResponse>(
            predicate: p =>
                p.IsActive != false &&
                (!request.PackagingType.HasValue || p.ProductPackagingType == request.PackagingType) &&
                (!request.ProductKind.HasValue || p.ProductKind == request.ProductKind) &&
                (!request.VolumeMl.HasValue || p.VolumeMl == request.VolumeMl) &&
                (!request.BrandId.HasValue || p.BrandId == request.BrandId),
            orderBy: q => q.OrderBy(p => p.ProductKind),
            include: q => q.Include(p => p.Brand),
            selector: e => _mapper.Map<GetAllProductsCreateOrderResponse>(e)
        );
        return response;
    }

    private static IOrderedQueryable<Product> OrderBy(IQueryable<Product> query, string sortBy, bool descending)
    {
        sortBy = sortBy?.Trim().ToLower() ?? "createdate";

        return (sortBy, descending) switch
        {
            ("name", false) => query.OrderBy(s => s.Name),
            ("name", true) => query.OrderByDescending(s => s.Name),
            ("code", false) => query.OrderBy(s => s.Code),
            ("code", true) => query.OrderByDescending(s => s.Code),
            ("quantity", false) => query.OrderBy(s => s.Quantity),
            ("quantity", true) => query.OrderByDescending(s => s.Quantity),
            ("volumeml", false) => query.OrderBy(s => s.VolumeMl),
            ("volumeml", true) => query.OrderByDescending(s => s.VolumeMl),
            ("price", false) => query.OrderBy(s => s.Price),
            ("price", true) => query.OrderByDescending(s => s.Price),
            ("brand", false) => query.OrderBy(s => s.Brand.Name),
            ("brand", true) => query.OrderByDescending(s => s.Brand.Name),
            (_, false) => query.OrderBy(s => s.CreateDate),
            (_, true) => query.OrderByDescending(s => s.CreateDate)
        };
    }

    public async Task<GetProductResponse> GetProductByIdAsync(Guid productId)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();
        var product = await productRepo.SingleOrDefaultAsync(
            predicate: p => p.Id == productId && p.IsActive != false,
            include: query => query.Include(p => p.ProductBatches).Include(p => p.Brand)
        ) ?? throw new NotFoundException($"Product {productId} not found");
        var response = _mapper.Map<GetProductResponse>(product);
        return response;
    }

    public async Task AddStockAsync(IEnumerable<AddProductStockRequest> requests)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();
        var batchRepo = _unitOfWork.GetRepository<ProductBatch>();

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var productIds = requests.Select(x => x.ProductId).Distinct().ToList();

            var products = await productRepo
                .GetListAsync(predicate: p => productIds.Contains(p.Id));

            var productDict = products.ToDictionary(p => p.Id);

            foreach (var request in requests)
            {
                if (!productDict.TryGetValue(request.ProductId, out var product))
                    throw new NotFoundException($"Product {request.ProductId} not found");

                var existingBatches = await batchRepo
                    .GetListAsync(predicate: b => b.ProductId == product.Id);

                var batchDict = existingBatches.ToDictionary(
                    b => (
                       b.ManuFactureDate.HasValue
                            ? DateOnly.FromDateTime(b.ManuFactureDate.Value)
                            : (DateOnly?)null,
                        b.ExpiryDate.HasValue
                            ? DateOnly.FromDateTime(b.ExpiryDate.Value)
                            : (DateOnly?)null
                    )
                );

                foreach (var batchRequest in request.ProductBatch)
                {
                    if (batchRequest.Quantity <= 0)
                        throw new BadRequestException("Quantity must be greater than zero");

                    var (manufactureDate, expiryDate) =
                        NormalizeDates(batchRequest, product.LifeSpan);

                    var key = (manufactureDate, expiryDate);

                    if (batchDict.TryGetValue(key, out var existingBatch))
                    {
                        existingBatch.Quantity += batchRequest.Quantity;
                        batchRepo.Update(existingBatch);
                    }
                    else
                    {
                        var newBatch = new ProductBatch
                        {
                            ProductId = product.Id,
                            ManuFactureDate = DateTime.SpecifyKind(
                            manufactureDate.ToDateTime(TimeOnly.MinValue),
                            DateTimeKind.Utc),

                            ExpiryDate = DateTime.SpecifyKind(
                            expiryDate.ToDateTime(TimeOnly.MinValue),
                            DateTimeKind.Utc),
                            Quantity = batchRequest.Quantity
                        };

                        await batchRepo.InsertAsync(newBatch);

                        batchDict[key] = newBatch;
                    }

                    product.Quantity += batchRequest.Quantity;
                }
                productRepo.Update(product);
            }
        });
    }

    private static (DateOnly manufactureDate, DateOnly expiryDate) NormalizeDates(AddProductBatchRequest request, int lifeSpanDays)
    {
        DateOnly? manufactureDate = request.ManuFactureDate.HasValue
            ? DateOnly.FromDateTime(request.ManuFactureDate.Value)
            : null;

        DateOnly? expiryDate = request.ExpiryDate.HasValue
            ? DateOnly.FromDateTime(request.ExpiryDate.Value)
            : null;

        if (manufactureDate.HasValue && expiryDate.HasValue)
        {
            var expectedExpiry = manufactureDate.Value.AddDays(lifeSpanDays);

            if (expectedExpiry != expiryDate.Value)
                throw new BusinessException("ExpiryDate does not match product lifespan");

            return (manufactureDate.Value, expiryDate.Value);
        }

        if (manufactureDate.HasValue)
        {
            var expiry = manufactureDate.Value.AddDays(lifeSpanDays);
            return (manufactureDate.Value, expiry);
        }

        if (expiryDate.HasValue)
        {
            var manufacture = expiryDate.Value.AddDays(-lifeSpanDays);
            return (manufacture, expiryDate.Value);
        }

        throw new BadRequestException("Either ManufactureDate or ExpiryDate must be provided");
    }

    public async Task<PagingResponse<GetProductBatchesResponse>> GetProductBatchesAsync(
        Guid productId,
        bool? isNewest,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var batchRepo = _unitOfWork.GetRepository<ProductBatch>();

        Expression<Func<ProductBatch, bool>> predicate =
            p => p.ProductId == productId && p.IsActive != false;

        if (isNewest == true)
        {
            return await batchRepo.GetPagingListAsync<GetProductBatchesResponse>(
                predicate: predicate,
                orderBy: q => q.OrderByDescending(p => p.ManuFactureDate),
                selector: e => _mapper.Map<GetProductBatchesResponse>(e),
                page: pageNumber,
                size: pageSize
            );
        }

        return await batchRepo.GetPagingListAsync<GetProductBatchesResponse>(
            predicate: predicate,
            orderBy: q => q.OrderBy(p => p.ExpiryDate),
            selector: e => _mapper.Map<GetProductBatchesResponse>(e),
            page: pageNumber,
            size: pageSize
        );
    }

    //public async Task<IEnumerable<GetProductBatchesResponse>> GetProductBatchesAsync(Guid productId, bool? isNewest)
    //{
    //    var batchRepo = _unitOfWork.GetRepository<ProductBatch>();

    //    if (isNewest == true)
    //    {
    //        var entity = await batchRepo.SingleOrDefaultAsync(
    //            predicate: p => p.ProductId == productId && p.IsActive != false,
    //            orderBy: q => q.OrderByDescending(p => p.ManuFactureDate)
    //        );

    //        if (entity == null)
    //            return Enumerable.Empty<GetProductBatchesResponse>();

    //        return new[]
    //        {
    //        _mapper.Map<GetProductBatchesResponse>(entity)
    //        };
    //    }

    //    return await batchRepo.GetListAsync(
    //        predicate: p => p.ProductId == productId && p.IsActive != false,
    //        orderBy: q => q.OrderBy(p => p.ExpiryDate),
    //        selector: e => _mapper.Map<GetProductBatchesResponse>(e)
    //    );
    //}

    public async Task<InventoryDashboardResponse> GetInventoryDashboardAsync()
    {
        var productRepo = _unitOfWork.GetRepository<Product>();

        var query = productRepo.GetQueryable();

        var totalProducts = await query.CountAsync();

        var lowStock = await query.CountAsync(p => p.Quantity < 30); 

        var totalBrands = await query
            .Where(p => p.IsActive == true && p.Quantity > 0)
            .Select(p => p.BrandId)
            .Distinct()
            .CountAsync();

        return new InventoryDashboardResponse
        {
            TotalProducts = totalProducts,
            LowStockProducts = lowStock,
            TotalBrands = totalBrands
        };
    }
}
