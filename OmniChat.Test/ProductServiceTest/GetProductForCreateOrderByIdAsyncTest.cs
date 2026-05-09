using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Product;
using OmniChat.Infrastructure.Dtos.Responses.Product;
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

public class GetProductForCreateOrderByIdAsyncTest
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
    public async Task GetProductForCreateOrderByIdAsync_ShouldReturnProducts_WhenValid()
    {
        var repo = SetupRepository<Product>();

        var expected = new List<GetAllProductsCreateOrderResponse>
        {
            new GetAllProductsCreateOrderResponse
            {
                Id = Guid.NewGuid(),
                Name = "Milk Tea"
            }
        };

        repo.Setup(r => r.GetListAsync<GetAllProductsCreateOrderResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsCreateOrderResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()
        )).ReturnsAsync(expected);

        var service = CreateService();

        var result = await service.GetProductForCreateOrderByIdAsync(
            new GetAllProductsCreateOrderQueryRequest()
        );

        Assert.NotNull(result);

        Assert.Single(result);

        repo.Verify(r => r.GetListAsync<GetAllProductsCreateOrderResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsCreateOrderResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()
        ), Times.Once);
    }

    [Fact]
    public async Task GetProductForCreateOrderByIdAsync_ShouldReturnEmpty_WhenNoProducts()
    {
        var repo = SetupRepository<Product>();

        repo.Setup(r => r.GetListAsync<GetAllProductsCreateOrderResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsCreateOrderResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()
        )).ReturnsAsync(new List<GetAllProductsCreateOrderResponse>());

        var service = CreateService();

        var result = await service.GetProductForCreateOrderByIdAsync(
            new GetAllProductsCreateOrderQueryRequest()
        );

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProductForCreateOrderByIdAsync_ShouldHandleNullRequest()
    {
        var repo = SetupRepository<Product>();

        repo.Setup(r => r.GetListAsync<GetAllProductsCreateOrderResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsCreateOrderResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()
        )).ReturnsAsync(new List<GetAllProductsCreateOrderResponse>());

        var service = CreateService();

        var result = await service.GetProductForCreateOrderByIdAsync(null);

        Assert.NotNull(result);

        repo.Verify(r => r.GetListAsync<GetAllProductsCreateOrderResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsCreateOrderResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()
        ), Times.Once);
    }

    [Fact]
    public async Task GetProductForCreateOrderByIdAsync_ShouldApplyFilters()
    {
        var repo = SetupRepository<Product>();

        repo.Setup(r => r.GetListAsync<GetAllProductsCreateOrderResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsCreateOrderResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()
        )).ReturnsAsync(new List<GetAllProductsCreateOrderResponse>());

        var service = CreateService();

        await service.GetProductForCreateOrderByIdAsync(
            new GetAllProductsCreateOrderQueryRequest
            {
                PackagingType = PackagingType.Bottle,
                ProductKind = ProductKind.Sugar,
                VolumeMl = 500,
                BrandId = Guid.NewGuid()
            });

        repo.Verify(r => r.GetListAsync<GetAllProductsCreateOrderResponse>(
            It.IsAny<Expression<Func<Product, GetAllProductsCreateOrderResponse>>>(),
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()
        ), Times.Once);
    }
}
