using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Product;
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

public class GetProductsAsyncTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<ProductService>> _loggerMock = new();
    protected readonly Mock<IR2StorageService> _storageMock = new();

    protected ProductService CreateService()
    {
        return new ProductService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _storageMock.Object
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
    public async Task GetProductsAsync_ShouldReturnPagingResponse_WhenValid()
    {
        var repo = SetupRepository<Product>();

        var expected = new PagingResponse<GetAllProductsResponse>
        {
            Items = new List<GetAllProductsResponse>
            {
                new GetAllProductsResponse
                {
                    Id = Guid.NewGuid(),
                    Name = "Milk Tea",
                    Code = "MT-CD-500"
                }
            },
            Meta = new PaginationMeta
            {
                CurrentPage = 1,
                PageSize = 20,
                TotalItems = 1,
                TotalPages = 1
            }
        };

        repo.Setup(r => r.GetPagingListAsync<GetAllProductsResponse>(
    It.IsAny<Expression<Func<Product, GetAllProductsResponse>>>(),
    It.IsAny<Expression<Func<Product, bool>>>(),
    It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
    It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
    It.IsAny<int>(),
    It.IsAny<int>()
)).ReturnsAsync(expected);

        var service = CreateService();

        var result = await service.GetProductsAsync(
            PackagingType: null,
            ProductKind: null,
            VolumeMl: null,
            BrandId: null,
            search: "Milk",
            pageNumber: 1,
            pageSize: 20
        );

        Assert.NotNull(result);

        Assert.Single(result.Items);

        Assert.Equal(1, result.Meta.TotalItems);

        repo.Verify(r => r.GetPagingListAsync<GetAllProductsResponse>(
    It.IsAny<Expression<Func<Product, GetAllProductsResponse>>>(),
    It.IsAny<Expression<Func<Product, bool>>>(),
    It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
    It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
    1,
    20
), Times.Once);
    }

    [Fact]
    public async Task GetProductsAsync_ShouldFilterByPackagingType()
    {
        var repo = SetupRepository<Product>();

        var expected = new PagingResponse<GetAllProductsResponse>
        {
            Items = new List<GetAllProductsResponse>(),
            Meta = new PaginationMeta()
        };

        repo.Setup(r => r.GetPagingListAsync<GetAllProductsResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
        .ReturnsAsync(expected);

        var service = CreateService();

        await service.GetProductsAsync(
            PackagingType: PackagingType.Bottle,
            ProductKind: null,
            VolumeMl: null,
            BrandId: null,
            search: null);

        repo.Verify(r => r.GetPagingListAsync<GetAllProductsResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()),
        Times.Once);
    }

    [Fact]
    public async Task GetProductsAsync_ShouldFilterByProductKind()
    {
        var repo = SetupRepository<Product>();

        repo.Setup(r => r.GetPagingListAsync<GetAllProductsResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
        .ReturnsAsync(new PagingResponse<GetAllProductsResponse>());

        var service = CreateService();

        await service.GetProductsAsync(
            PackagingType: null,
            ProductKind: ProductKind.Sugar,
            VolumeMl: null,
            BrandId: null,
            search: null);

        repo.Verify(r => r.GetPagingListAsync<GetAllProductsResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()),
        Times.Once);
    }

    [Fact]
    public async Task GetProductsAsync_ShouldFilterByBrandId()
    {
        var repo = SetupRepository<Product>();

        repo.Setup(r => r.GetPagingListAsync<GetAllProductsResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
        .ReturnsAsync(new PagingResponse<GetAllProductsResponse>());

        var brandId = Guid.NewGuid();

        var service = CreateService();

        await service.GetProductsAsync(
            PackagingType: null,
            ProductKind: null,
            VolumeMl: null,
            BrandId: brandId,
            search: null);

        repo.Verify(r => r.GetPagingListAsync<GetAllProductsResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()),
        Times.Once);
    }

    [Fact]
    public async Task GetProductsAsync_ShouldFilterBySearch()
    {
        var repo = SetupRepository<Product>();

        repo.Setup(r => r.GetPagingListAsync<GetAllProductsResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
        .ReturnsAsync(new PagingResponse<GetAllProductsResponse>());

        var service = CreateService();

        await service.GetProductsAsync(
            PackagingType: null,
            ProductKind: null,
            VolumeMl: null,
            BrandId: null,
            search: "Milk");

        repo.Verify(r => r.GetPagingListAsync<GetAllProductsResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()),
        Times.Once);
    }

    [Fact]
    public async Task GetProductsAsync_ShouldReturnEmptyPaging_WhenNoProducts()
    {
        var repo = SetupRepository<Product>();

        var expected = new PagingResponse<GetAllProductsResponse>
        {
            Items = new List<GetAllProductsResponse>(),
            Meta = new PaginationMeta
            {
                TotalItems = 0,
                TotalPages = 0,
                CurrentPage = 1,
                PageSize = 20
            }
        };

        repo.Setup(r => r.GetPagingListAsync<GetAllProductsResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>(),
            It.IsAny<int>(),
            It.IsAny<int>()))
        .ReturnsAsync(expected);

        var service = CreateService();

        var result = await service.GetProductsAsync(
            null, null, null, null, null);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Meta.TotalItems);
    }
}