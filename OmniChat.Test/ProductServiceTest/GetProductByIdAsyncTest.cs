using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatch;
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

namespace OmniChat.Test.ProductServiceTest;

public class GetProductByIdAsyncTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<ProductService>> _loggerMock = new();
    protected readonly Mock<IR2StorageService> _storageMock = new();
    protected readonly Mock<IProductBatchAuditService> _auditMock = new();

    protected ProductService CreateService()
    {
        return new ProductService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _storageMock.Object,
            _auditMock.Object
        );
    }

    protected Mock<IGenericRepository<T>> SetupRepository<T>() where T : class
    {
        var repoMock = new Mock<IGenericRepository<T>>();

        _uowMock.Setup(x => x.GetRepository<T>())
            .Returns(repoMock.Object);

        return repoMock;
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnProduct_WhenFound()
    {
        var repo = SetupRepository<Product>();

        var productId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Name = "Milk Tea",
            IsActive = true
        };

        var mappedResponse = new GetProductResponse
        {
            Id = productId,
            Name = "Milk Tea"
        };

        repo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()
        )).ReturnsAsync(product);

        _mapperMock
            .Setup(m => m.Map<GetProductResponse>(product))
            .Returns(mappedResponse);

        var service = CreateService();

        var result = await service.GetProductByIdAsync(productId);

        Assert.NotNull(result);

        Assert.Equal(productId, result.Id);

        Assert.Equal("Milk Tea", result.Name);

        repo.Verify(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()
        ), Times.Once);

        _mapperMock.Verify(m =>
            m.Map<GetProductResponse>(product),
            Times.Once);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldThrowNotFound_WhenMissing()
    {
        var repo = SetupRepository<Product>();

        repo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()
        )).ReturnsAsync((Product)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetProductByIdAsync(Guid.NewGuid()));

        _mapperMock.Verify(m =>
            m.Map<GetProductResponse>(It.IsAny<Product>()),
            Times.Never);
    }

    [Fact]
    public async Task GetProductBatchesAsync_ShouldOrderByManufactureDateDescending_WhenIsNewestTrue()
    {
        var repo = SetupRepository<ProductBatch>();

        var productId = Guid.NewGuid();

        var expected = new PagingResponse<GetProductBatchesResponse>
        {
            Items = new List<GetProductBatchesResponse>
        {
            new GetProductBatchesResponse()
        },
            Meta = new PaginationMeta
            {
                CurrentPage = 1,
                PageSize = 20,
                TotalItems = 1,
                TotalPages = 1
            }
        };

        repo.Setup(r => r.GetPagingListAsync<GetProductBatchesResponse>(
            It.IsAny<Expression<Func<ProductBatch, GetProductBatchesResponse>>>(),
            It.IsAny<Expression<Func<ProductBatch, bool>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IOrderedQueryable<ProductBatch>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IIncludableQueryable<ProductBatch, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
        .ReturnsAsync(expected);

        var service = CreateService();

        var result = await service.GetProductBatchesAsync(
            productId,
            true,
            1,
            20);

        Assert.Equal(expected, result);

        repo.Verify(r => r.GetPagingListAsync<GetProductBatchesResponse>(
            It.IsAny<Expression<Func<ProductBatch, GetProductBatchesResponse>>>(),
            It.IsAny<Expression<Func<ProductBatch, bool>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IOrderedQueryable<ProductBatch>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IIncludableQueryable<ProductBatch, object>>>(),
            1,
            20),
            Times.Once);
    }

    [Fact]
    public async Task GetProductBatchesAsync_ShouldOrderByExpiryDate_WhenIsNewestFalse()
    {
        var repo = SetupRepository<ProductBatch>();

        var productId = Guid.NewGuid();

        var expected = new PagingResponse<GetProductBatchesResponse>
        {
            Items = new List<GetProductBatchesResponse>(),
            Meta = new PaginationMeta
            {
                CurrentPage = 1,
                PageSize = 20,
                TotalItems = 0,
                TotalPages = 0
            }
        };

        repo.Setup(r => r.GetPagingListAsync<GetProductBatchesResponse>(
            It.IsAny<Expression<Func<ProductBatch, GetProductBatchesResponse>>>(),
            It.IsAny<Expression<Func<ProductBatch, bool>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IOrderedQueryable<ProductBatch>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IIncludableQueryable<ProductBatch, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
        .ReturnsAsync(expected);

        var service = CreateService();

        var result = await service.GetProductBatchesAsync(
            productId,
            false,
            1,
            20);

        Assert.Equal(expected, result);

        repo.Verify(r => r.GetPagingListAsync<GetProductBatchesResponse>(
            It.IsAny<Expression<Func<ProductBatch, GetProductBatchesResponse>>>(),
            It.IsAny<Expression<Func<ProductBatch, bool>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IOrderedQueryable<ProductBatch>>>(),
            It.IsAny<Func<IQueryable<ProductBatch>, IIncludableQueryable<ProductBatch, object>>>(),
            1,
            20),
            Times.Once);
    }
}
