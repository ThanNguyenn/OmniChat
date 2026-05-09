using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
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

public class DeleteProductAsyncTest
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

    protected void SetupTransaction()
    {
        _uowMock
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());
    }

    public static class ProductTestData
    {
        public static Product Create(Guid id)
        {
            return new Product
            {
                Id = id,
                IsActive = true
            };
        }
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldMarkInactive_AndCallUpdate()
    {
        var repo = SetupRepository<Product>();
        SetupTransaction();

        var productFromRepo = new Product
        {
            Id = Guid.NewGuid(),
            IsActive = true
        };

        repo.Setup(r => r.SingleOrDefaultAsync(
        It.IsAny<Expression<Func<Product, bool>>>(),
        It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
        It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()))
    .ReturnsAsync(productFromRepo);

        var service = CreateService();

        var result = await service.DeleteProductAsync(productFromRepo.Id);

        Assert.True(result);

        repo.Verify(r => r.Update(It.Is<Product>(p =>
            p == productFromRepo &&
            p.IsActive == false
        )), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldThrowNotFound_WhenProductMissing()
    {
        var repo = SetupRepository<Product>();
        SetupTransaction();

        repo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<Func<IQueryable<Product>, IIncludableQueryable<Product, object>>>()))
            .ReturnsAsync((Product?)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.DeleteProductAsync(Guid.NewGuid()));
    }
}