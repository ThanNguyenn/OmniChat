using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Staff;
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

namespace OmniChat.Test.StaffServiceTest;

public class CreateStaffAsyncTest
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

    protected void SetupTransaction(Func<Task<bool>>? callback = null)
    {
        _uowMock
            .Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
            .Returns<Func<Task<bool>>>(func => func());
    }
    protected Mock<IGenericRepository<T>> SetupRepository<T>() where T : class
    {
        var repoMock = new Mock<IGenericRepository<T>>();

        _uowMock.Setup(u => u.GetRepository<T>())
            .Returns(repoMock.Object);

        return repoMock;
    }

    protected void SetupStaffNotExists(Mock<IGenericRepository<Staff>> repo)
    {
        repo.Setup(r => r.SingleOrDefaultAsync(
        It.IsAny<Expression<Func<Staff, bool>>>(),
        It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
        It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
        .ReturnsAsync((Staff?)null);
    }

    [Fact]
    public async Task CreateStaffAsync_ShouldReturnTrue_WhenValid()
    {
        var staffRepo = SetupRepository<Staff>();
        var accountRepo = SetupRepository<Account>();

        SetupTransaction();
        SetupStaffNotExists(staffRepo);

        _mapperMock.Setup(m => m.Map<Staff>(It.IsAny<CreateStaffRequest>()))
            .Returns(new Staff());

        var service = CreateService();

        var result = await service.CreateStaffAsync(new CreateStaffRequest
        {
            Email = "test@mail.com",
            Phone = "123"
        });

        Assert.True(result);

        staffRepo.Verify(r => r.InsertAsync(It.IsAny<Staff>()), Times.Once);
        accountRepo.Verify(r => r.InsertAsync(It.IsAny<Account>()), Times.Once);
    }

    [Fact]
    public async Task CreateStaffAsync_ShouldThrowBusinessException_WhenStaffAlreadyExists()
    {
        var staffRepo = SetupRepository<Staff>();

        SetupTransaction();

        staffRepo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Staff, bool>>>(),
            It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
            It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
            .ReturnsAsync(new Staff()); // existing staff

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateStaffAsync(new CreateStaffRequest
            {
                Email = "test@mail.com",
                Phone = "123"
            }));
    }

    [Fact]
    public async Task CreateStaffAsync_ShouldSkipIntentAssignment_WhenIntentTypesNull()
    {
        var staffRepo = SetupRepository<Staff>();
        var accountRepo = SetupRepository<Account>();

        SetupTransaction();
        SetupStaffNotExists(staffRepo);

        _mapperMock.Setup(m => m.Map<Staff>(It.IsAny<CreateStaffRequest>()))
            .Returns(new Staff());

        var service = CreateService();

        var request = new CreateStaffRequest
        {
            Email = "test@mail.com",
            Phone = "123",
            StaffIntentTypes = null
        };

        var result = await service.CreateStaffAsync(request);

        Assert.True(result);

        staffRepo.Verify(r => r.InsertAsync(It.IsAny<Staff>()), Times.Once);
    }

    [Fact]
    public async Task CreateStaffAsync_ShouldSkipIntentAssignment_WhenIntentTypesEmpty()
    {
        var staffRepo = SetupRepository<Staff>();
        var accountRepo = SetupRepository<Account>();

        SetupTransaction();
        SetupStaffNotExists(staffRepo);

        staffRepo.Setup(r => r.InsertAsync(It.IsAny<Staff>()))
            .Callback<Staff>(s => s.Id = Guid.NewGuid())
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(m => m.Map<Staff>(It.IsAny<CreateStaffRequest>()))
            .Returns(new Staff());

        var service = CreateService();

        var request = new CreateStaffRequest
        {
            Email = "test@mail.com",
            Phone = "123",
            StaffIntentTypes = new List<AssignStaffToIntentTypeRequest>() // EMPTY
        };

        var result = await service.CreateStaffAsync(request);

        Assert.True(result);

        staffRepo.Verify(r => r.InsertAsync(It.IsAny<Staff>()), Times.Once);
        accountRepo.Verify(r => r.InsertAsync(It.IsAny<Account>()), Times.Once);
    }
}
