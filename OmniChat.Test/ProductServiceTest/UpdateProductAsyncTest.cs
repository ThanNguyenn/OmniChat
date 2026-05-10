using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Product;
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

public class UpdateProductAsyncTest
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
    protected void SetupTransaction()
    {
        _uowMock
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());
    }

    protected Mock<IGenericRepository<T>> SetupRepository<T>() where T : class
    {
        var repoMock = new Mock<IGenericRepository<T>>();

        _uowMock.Setup(u => u.GetRepository<T>())
            .Returns(repoMock.Object);

        return repoMock;
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnTrue_WhenProductExists()
    {
        var repo = SetupRepository<Product>();
        SetupTransaction();

        var productId = Guid.NewGuid();

        var existing = new Product
        {
            Id = productId,
            Name = "Old"
        };

        repo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()))
            .ReturnsAsync(existing);

        var service = CreateService();

        var result = await service.UpdateProductAsync(productId, new UpdateProductRequest
        {
            Name = "New Name",
            Price = 100
        });

        Assert.True(result);

        repo.Verify(r => r.Update(existing), Times.Once);
        _mapperMock.Verify(m => m.Map(It.IsAny<UpdateProductRequest>(), existing), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldThrowNotFound_WhenProductNotExists()
    {
        var repo = SetupRepository<Product>();
        SetupTransaction();

        repo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()))
            .ReturnsAsync((Product)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateProductAsync(Guid.NewGuid(), new UpdateProductRequest()));
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldPartiallyUpdate_WhenOnlySomeFieldsProvided()
    {
        var repo = SetupRepository<Product>();
        SetupTransaction();

        var productId = Guid.NewGuid();

        var existing = new Product
        {
            Id = productId,
            Name = "Old Name",
            Description = "Old Desc",
            Price = 50
        };

        repo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Product, bool>>>(),
            It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
            It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()))
            .ReturnsAsync(existing);

        _mapperMock
            .Setup(m => m.Map(It.IsAny<UpdateProductRequest>(), existing))
            .Callback<UpdateProductRequest, Product>((req, entity) =>
            {
                if (req.Name != null)
                    entity.Name = req.Name;

                if (req.Price.HasValue)
                    entity.Price = req.Price.Value;
            });

        var service = CreateService();

        var result = await service.UpdateProductAsync(productId, new UpdateProductRequest
        {
            Name = "New Name"
        });

        Assert.True(result);

        Assert.Equal("New Name", existing.Name);
        Assert.Equal(50, existing.Price);

        repo.Verify(r => r.Update(existing), Times.Once);
    }
}
