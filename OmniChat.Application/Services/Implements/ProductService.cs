using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Product;
using OmniChat.Infrastructure.Dtos.Requests.ProductBatch;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class ProductService : BaseService<ProductService>, IProductService
{
    public ProductService(IUnitOfWork<OmniChatDbContext> unitOfWork,
        ILogger<ProductService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
        : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task<bool> CreateProductAsync(CreateProductRequest createProductRequest)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();
        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var lastProduct = await productRepo
            .GetQueryable()
            .OrderByDescending(p => p.Code)
            .FirstOrDefaultAsync();
            int lastCode =
                lastProduct != null &&
                int.TryParse(lastProduct.Code.AsSpan(3), out var codeValue)
                    ? codeValue
                    : 0;
            var newCode = GenerateProductCode(lastCode);

            var imageUrl = "https://via.placeholder.com";
            //imagelogic

            //end


            var newProduct = _mapper.Map<Product>(createProductRequest);
            newProduct.Code = newCode;
            newProduct.ImageUrl = imageUrl;
            await productRepo.InsertAsync(newProduct);
        });
        return true;
    }

    private string GenerateProductCode(int lastCode)
    {
        return (lastCode + 1).ToString("D6");
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

    public async Task<PagingResponse<GetAllProductsResponse>> GetProductsAsync(string? search, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();
        var response = await productRepo.GetPagingListAsync<GetAllProductsResponse>(
                predicate: p => p.IsActive != false && (string.IsNullOrEmpty(search) || p.Name.Contains(search) || p.Code.Contains(search)),
                orderBy: q => OrderBy(q, sortBy, descending),
                selector: e => _mapper.Map<GetAllProductsResponse>(e),
                page: pageNumber,
                size: pageSize
                );
        return response;

    }
    private static IOrderedQueryable<Product> OrderBy(IQueryable<Product> query, string sortBy, bool descending)
    {
        sortBy = sortBy?.Trim().ToLower() ?? "id";

        Expression<Func<Product, object>> keySelector = sortBy switch
        {
            "name" => s => s.Name,
            "code" => s => s.Code,
            "quantity" => s => s.Quantity,
            "volumeml" => s => s.VolumeMl,
            "price" => s => s.Price,
            "brand" => s => s.Brand,
            _ => s => s.Id
        };

        return descending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }
    public async Task<GetProductResponse> GetProductByIdAsync(Guid productId)
    {
        var productRepo = _unitOfWork.GetRepository<Product>();
        var product = await productRepo.SingleOrDefaultAsync(
            predicate: p => p.Id == productId && p.IsActive != false,
            include: query => query.Include(p => p.ProductBatches)
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
                            ManuFactureDate = manufactureDate.ToDateTime(TimeOnly.MinValue),
                            ExpiryDate = expiryDate.ToDateTime(TimeOnly.MinValue),
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
}
