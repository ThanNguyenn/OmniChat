using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Utils;
using OmniChat.Infrastructure.Dtos.Requests.Product;
using OmniChat.Infrastructure.Dtos.Requests.ProductBatch;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatch;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Helper;
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
    private readonly IProductBatchAuditService _productBatchAuditService;
    public ProductService(
        IUnitOfWork<OmniChatDbContext> unitOfWork,
        ILogger<ProductService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IR2StorageService storageService,
        IProductBatchAuditService productBatchAuditService)
    : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _storageService = storageService;
        _productBatchAuditService = productBatchAuditService;
    }

    public async Task<bool> CreateProductAsync(CreateProductRequest createProductRequest)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();

        Product newProduct = null;

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            newProduct = _mapper.Map<Product>(createProductRequest);

            newProduct.Code = ProductHelper.GenerateSku(
            newProduct.Name,
            newProduct.ProductKind,
            newProduct.VolumeMl
            );

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
                throw new NotFoundException("Không tìm thấy sản phẩm");
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
                throw new NotFoundException("Không tìm thấy sản phẩm");
            }
            _mapper.Map(updateProductRequest, existingProduct);
            productRepo.Update(existingProduct);
        });
        return true;
    }

    public async Task<bool> UpdateProductImageAsync(Guid productId, UpdateProductImageRequest request)
    {
        if (request?.Image == null || request.Image.Length == 0)
            throw new BusinessException("File không hỗ trợ");

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

    public async Task<PagingResponse<GetAllProductsResponse>> GetProductsAsync(PackagingType? PackagingType, ProductKind? ProductKind, double? VolumeMl, Guid? BrandId, string? search, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();
        var response = await productRepo.GetPagingListAsync<GetAllProductsResponse>(
                predicate: p =>
                    p.IsActive != false
                    && (PackagingType == null || p.ProductPackagingType == PackagingType)
                    && (ProductKind == null || p.ProductKind == ProductKind)
                    && (VolumeMl == null || p.VolumeMl == VolumeMl)
                    && (BrandId == null || p.BrandId == BrandId)
                    && (string.IsNullOrEmpty(search)
                        || p.Name.Contains(search)
                        || p.Code.Contains(search)),
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
        ) ?? throw new NotFoundException($"Không tìm thấy sản phẩm");
        var response = _mapper.Map<GetProductResponse>(product);
        return response;
    }

    public async Task AddStockAsync(IEnumerable<AddProductStockRequest> requests)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();
        var batchRepo = _unitOfWork.GetRepository<ProductBatch>();
        var staffRepo = _unitOfWork.GetRepository<Staff>();

        var suffixTracker = new Dictionary<string, int>();

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var productIds = requests.Select(x => x.ProductId).Distinct().ToList();

            var products = await productRepo
                .GetListAsync(predicate: p => productIds.Contains(p.Id));

            var productDict = products.ToDictionary(p => p.Id);

            foreach (var request in requests)
            {
                if (!productDict.TryGetValue(request.ProductId, out var product))
                    throw new NotFoundException($"Không tìm thấy sản phẩm");

                foreach (var batchRequest in request.ProductBatch)
                {
                    var (manufactureDate, expiryDate) =
                        NormalizeDates(batchRequest, product.LifeSpan);

                    var manufactureDateTime = DateTime.SpecifyKind(manufactureDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
                    var expiryDateTime = DateTime.SpecifyKind(expiryDate.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

                    string dateStr = expiryDateTime.ToString("yyyyMMdd");

                    if (!suffixTracker.TryGetValue(dateStr, out int nextSuffix))
                    {
                        var latestBatch = await batchRepo.SingleOrDefaultAsync(
                        predicate: b => b.Code.StartsWith($"LOT{dateStr}"),
                        orderBy: q => q.OrderByDescending(b => b.Code)
                        );

                        if (latestBatch != null)
                        {
                            Console.WriteLine($"[DEBUG LOG] Tìm thấy lô lớn nhất trong DB: {latestBatch.Code}");
                            var suffixStr = latestBatch.Code.Replace($"LOT{dateStr}", "");
                            Console.WriteLine($"[DEBUG LOG] Chuỗi sau khi Replace tiền tố: '{suffixStr}'");
                            if (string.IsNullOrEmpty(suffixStr))
                            {
                                nextSuffix = 1;
                            }

                            else if (int.TryParse(suffixStr, out int lastSuffix))
                            {
                                nextSuffix = lastSuffix + 1;
                            }
                            else
                            {
                                nextSuffix = 0;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[DEBUG LOG] Không tìm thấy lô nào trước đó cho ngày {dateStr}. Bắt đầu từ 0.");
                            nextSuffix = 0;
                        }
                    }

                    string generatedCode = BatchHelper.GenerateBatchCode(expiryDateTime, nextSuffix);
                    Console.WriteLine($"[DEBUG LOG] Mã CODE vừa được sinh ra từ Helper: {generatedCode} (Suffix: {nextSuffix})");
                    suffixTracker[dateStr] = nextSuffix + 1;

                    var newBatch = new ProductBatch
                    {
                        ProductId = product.Id,
                        ManuFactureDate = DateTime.SpecifyKind(
                            manufactureDate.ToDateTime(TimeOnly.MinValue),
                            DateTimeKind.Utc),

                        ExpiryDate = DateTime.SpecifyKind(
                            expiryDate.ToDateTime(TimeOnly.MinValue),
                            DateTimeKind.Utc),

                        Quantity = batchRequest.Quantity,

                        Code = generatedCode,
                    };
                    Console.WriteLine($"[DEBUG LOG] Chuỗi CODE nằm trong Entity trước khi Insert: {newBatch.Code}");

                    Console.WriteLine($"[DEBUG LOG 1 - TRƯỚC INSERT] newBatch Object -> Code: {newBatch.Code}, Quantity: {newBatch.Quantity}, Id: {newBatch.Id}");
                    
                    await batchRepo.InsertAsync(newBatch);

                    Console.WriteLine($"[DEBUG LOG 2 - SAU INSERT] newBatch Object -> Code: {newBatch.Code}, Quantity: {newBatch.Quantity}, Id: {newBatch.Id}");
                    await _unitOfWork.CommitAsync();

                    product.Quantity += batchRequest.Quantity;

                    var accountId = _httpContextAccessor.HttpContext?.User.GetUserId();

                    var staff = await staffRepo.SingleOrDefaultAsync(
                        predicate: s => s.AccountId == accountId);

                    var oldValue = 0;
                    var newValue = newBatch.Quantity;

                    await _productBatchAuditService.AddAsync(
                        newBatch.Id,
                        oldValue,
                        newValue,
                        staff.Id
                    );
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
                throw new BusinessException("Ngày hết hạn không khớp với vòng đời sản phẩm");

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

        throw new BadRequestException("Ngày hết hạn không khớp với vòng đời sản phẩm");
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

    public async Task UpdateBatchExpiryAsync()
    {
        var batchRepo = _unitOfWork.GetRepository<ProductBatch>();
        var now = DateTime.UtcNow;
        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var expiredBatches = await batchRepo.GetListAsync(
            predicate: b =>
                b.ExpiryDate.HasValue &&
                b.ExpiryDate <= now &&
                (b.IsExpired == null || b.IsExpired == false)
        );

            foreach (var batch in expiredBatches)
            {
                batch.IsExpired = true;
                batch.IsActive = false;
                batchRepo.Update(batch);
                await _productBatchAuditService.AddAsync(
                    batch.Id,
                    batch.Quantity,
                    0,
                    Guid.Empty
                );
            }
        });
    }
}