using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Product;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace OmniChat.Test.ProductServiceTest;

public class CreateProductAsyncTest
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
    public async Task CreateProductAsync_ShouldInsertProduct_AndUploadImage_WhenImageProvided()
    {
        var productRepo = SetupRepository<Product>();
        SetupTransaction();

        var productId = Guid.NewGuid();

        var fileMock = new Mock<IFormFile>();
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        fileMock.Setup(f => f.Length).Returns(3);
        fileMock.Setup(f => f.FileName).Returns("product.jpg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);

        _mapperMock
            .Setup(m => m.Map<Product>(It.IsAny<CreateProductRequest>()))
            .Returns((CreateProductRequest r) => new Product
            {
                Id = productId,
                Name = r.Name,
                ProductKind = r.ProductKind,
                VolumeMl = r.VolumeMl ?? 100
            });

        _storageMock
            .Setup(s => s.UploadUpdatedImageAsync(
                It.IsAny<Stream>(),
                "product.jpg",
                "products",
                It.IsAny<Guid>()))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.CreateProductAsync(new CreateProductRequest
        {
            Image = fileMock.Object,
            Name = "Test Product",
            ProductKind = ProductKind.Sugar,
            VolumeMl = 100
        });

        Assert.True(result);

        productRepo.Verify(r =>
            r.InsertAsync(It.IsAny<Product>()),
            Times.Once);

        _storageMock.Verify(s =>
            s.UploadUpdatedImageAsync(
                It.IsAny<Stream>(),
                "product.jpg",
                "products",
                It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldInsertProduct_WithoutUploading_WhenNoImage()
    {
        var productRepo = SetupRepository<Product>();
        SetupTransaction();

        _mapperMock
            .Setup(m => m.Map<Product>(It.IsAny<CreateProductRequest>()))
            .Returns((CreateProductRequest r) => new Product
            {
                Id = Guid.NewGuid(),
                Name = r.Name,
                ProductKind = r.ProductKind,
                VolumeMl = r.VolumeMl ?? 100
            });

        var service = CreateService();

        var result = await service.CreateProductAsync(new CreateProductRequest
        {
            Image = null,
            Name = "Test Product",
            ProductKind = ProductKind.Sugar,
            VolumeMl = 100
        });

        Assert.True(result);

        productRepo.Verify(r =>
            r.InsertAsync(It.IsAny<Product>()),
            Times.Once);

        _storageMock.Verify(s =>
            s.UploadUpdatedImageAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>()),
            Times.Never);
    }
}