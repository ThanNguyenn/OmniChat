using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Staff;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.StaffServiceTest;

public class UploadStaffImageTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<StaffService>> _loggerMock = new();
    protected readonly Mock<IR2StorageService> _storageMock = new();

    protected StaffService CreateService()
    {
        return new StaffService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _storageMock.Object
        );
    }

    [Fact]
    public async Task UploadStaffImage_ShouldReturnTrue_WhenValidFile()
    {
        var staffId = Guid.NewGuid();

        var fileMock = new Mock<IFormFile>();
        var content = new MemoryStream(new byte[] { 1, 2, 3 });

        fileMock.Setup(f => f.Length).Returns(3);
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(content);

        _storageMock
            .Setup(s => s.UploadUpdatedImageAsync(
                It.IsAny<Stream>(),
                "test.jpg",
                "staffs",
                staffId))
            .ReturnsAsync(true);

        var service = CreateService();

        var result = await service.UploadStaffImage(staffId, new UploadStaffImageRequest
        {
            Image = fileMock.Object
        });

        Assert.True(result);

        _storageMock.Verify(s =>
            s.UploadUpdatedImageAsync(
                It.IsAny<Stream>(),
                "test.jpg",
                "staffs",
                staffId),
            Times.Once);
    }

    [Fact]
    public async Task UploadStaffImage_ShouldThrowBusinessException_WhenFileIsNull()
    {
        var storageMock = new Mock<IR2StorageService>();

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.UploadStaffImage(Guid.NewGuid(), new UploadStaffImageRequest
            {
                Image = null
            }));
    }
}
