using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Product;
using OmniChat.Infrastructure.Dtos.Requests.ProductBatch;
using OmniChat.Infrastructure.Exceptions;
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

public class AddStockAsyncTest
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

    protected void SetupTransaction()
    {
        _uowMock
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());
    }

    [Fact]
    public async Task AddStockAsync_ShouldInsertBatch_AndUpdateQuantity()
    {
        var productRepo = SetupRepository<Product>();
        var batchRepo = SetupRepository<ProductBatch>();

        SetupTransaction();

        var productId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            Quantity = 10,
            LifeSpan = 30
        };

        productRepo.Setup(r => r.GetListAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()))
        .ReturnsAsync(new List<Product> { product });

        var service = CreateService();

        var manufactureDate = new DateTime(2026, 1, 1);

        await service.AddStockAsync(new[]
        {
        new AddProductStockRequest
        {
            ProductId = productId,
            ProductBatch = new List<AddProductBatchRequest>
            {
                new AddProductBatchRequest
                {
                    ManuFactureDate = manufactureDate,
                    Quantity = 5
                }
            }
        }
    });

        Assert.Equal(15, product.Quantity);

        batchRepo.Verify(r => r.InsertAsync(It.Is<ProductBatch>(b =>
            b.ProductId == productId &&
            b.Quantity == 5
        )), Times.Once);

        productRepo.Verify(r =>
            r.Update(It.Is<Product>(p =>
                p.Id == productId &&
                p.Quantity == 15)),
            Times.Once);
        _auditMock.Verify(a =>
            a.AddAsync(It.IsAny<Guid>(), 5, It.IsAny<Guid?>()),
            Times.Once);
    }

    [Fact]
    public async Task AddStockAsync_ShouldThrowBusinessException_WhenDatesMismatch()
    {
        var productRepo = SetupRepository<Product>();
        var batchRepo = SetupRepository<ProductBatch>();

        SetupTransaction();

        var productId = Guid.NewGuid();

        productRepo.Setup(r => r.GetListAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()))
        .ReturnsAsync(new List<Product>
        {
        new Product
        {
            Id = productId,
            LifeSpan = 30
        }
        });

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.AddStockAsync(new[]
            {
            new AddProductStockRequest
            {
                ProductId = productId,
                ProductBatch = new List<AddProductBatchRequest>
                {
                    new AddProductBatchRequest
                    {
                        ManuFactureDate = new DateTime(2026, 1, 1),
                        ExpiryDate = new DateTime(2026, 1, 10),
                        Quantity = 5
                    }
                }
            }
            }));
    }

    [Fact]
    public async Task AddStockAsync_ShouldThrowNotFound_WhenProductMissing()
    {
        var productRepo = SetupRepository<Product>();

        SetupTransaction();

        productRepo.Setup(r => r.GetListAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()))
        .ReturnsAsync(new List<Product>());

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AddStockAsync(new[]
            {
            new AddProductStockRequest
            {
                ProductId = Guid.NewGuid(),
                ProductBatch = new List<AddProductBatchRequest>
                {
                    new AddProductBatchRequest
                    {
                        ManuFactureDate = DateTime.UtcNow,
                        Quantity = 5
                    }
                }
            }
            }));
    }
}
