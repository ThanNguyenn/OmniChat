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

public class UpdateStaffAsyncTest
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

    protected void SetupTransaction()
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

    protected void SetupQueryable<T>(
        Mock<IGenericRepository<T>> repo,
        IQueryable<T> data) where T : class
    {
        repo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<T, bool>>>(),
                It.IsAny<Func<IQueryable<T>, IQueryable<T>>>(),
                It.IsAny<bool>()))
            .Returns(new TestAsyncEnumerable<T>(data));
    }

    protected void SetupStaff(Mock<IGenericRepository<Staff>> repo, Staff staff)
    {
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(staff);
    }

    protected void SetupStaffNotFound(Mock<IGenericRepository<Staff>> repo)
    {
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Staff?)null);
    }

    public static class StaffTestData
    {
        public static Staff CreateStaff(
            Guid? id = null,
            string email = "old@mail.com",
            string phone = "123")
        {
            return new Staff
            {
                Id = id ?? Guid.NewGuid(),
                Email = email,
                Phone = phone,
                StaffIntentTypes = new List<StaffIntentType>()
            };
        }
    }

    [Fact]
    public async Task UpdateStaffAsync_ShouldReturnTrue_WhenValid()
    {
        var staffRepo = SetupRepository<Staff>();
        SetupTransaction();

        var staff = StaffTestData.CreateStaff();

        SetupStaff(staffRepo, staff);

        SetupQueryable(staffRepo, new List<Staff>().AsQueryable());

        var service = CreateService();

        var result = await service.UpdateStaffAsync(staff.Id, new UpdateStaffRequest
        {
            Email = "new@mail.com",
            Phone = "999"
        });

        Assert.True(result);
        staffRepo.Verify(r => r.Update(It.IsAny<Staff>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStaffAsync_ShouldThrowNotFoundException_WhenStaffNotExists()
    {
        var staffRepo = SetupRepository<Staff>();
        SetupTransaction();

        SetupStaffNotFound(staffRepo);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateStaffAsync(Guid.NewGuid(), new UpdateStaffRequest()));
    }   

    [Fact]
    public async Task UpdateStaffAsync_ShouldThrowBusinessException_WhenDuplicateEmailExists()
    {
        var staffRepo = SetupRepository<Staff>();
        SetupTransaction();

        var staffId = Guid.NewGuid();

        SetupStaff(staffRepo, StaffTestData.CreateStaff(staffId));

        SetupQueryable(staffRepo, new List<Staff>
        {
            StaffTestData.CreateStaff(email: "new@mail.com")
        }.AsQueryable());

        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.UpdateStaffAsync(staffId, new UpdateStaffRequest
            {
                Email = "new@mail.com"
            }));
    }

    [Fact]
    public async Task UpdateStaffAsync_ShouldSkipDuplicateCheck_WhenEmailAndPhoneUnchanged()
    {
        var staffRepo = SetupRepository<Staff>();
        SetupTransaction();

        var staffId = Guid.NewGuid();

        var staff = StaffTestData.CreateStaff(
            id: staffId,
            email: "same@mail.com",
            phone: "123");

        SetupStaff(staffRepo, staff);

        var service = CreateService();

        var result = await service.UpdateStaffAsync(staffId, new UpdateStaffRequest
        {
            Email = "same@mail.com",
            Phone = "123"
        });

        Assert.True(result);
        staffRepo.Verify(r => r.Update(It.IsAny<Staff>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStaffAsync_ShouldCallSyncIntents_WhenIntentTypesProvided()
    {
        var staffRepo = SetupRepository<Staff>();
        var intentRepo = SetupRepository<IntentType>();
        var staffIntentRepo = SetupRepository<StaffIntentType>();

        SetupTransaction();

        var staffId = Guid.NewGuid();
        var intentId = Guid.NewGuid();

        SetupStaff(staffRepo, StaffTestData.CreateStaff(staffId));

        SetupQueryable(staffRepo, new List<Staff>().AsQueryable());

        intentRepo
    .Setup(r => r.GetListAsync(
        It.IsAny<Expression<Func<IntentType, bool>>>(),
        It.IsAny<Func<IQueryable<IntentType>, IOrderedQueryable<IntentType>>>(),
        It.IsAny<Func<IQueryable<IntentType>, IIncludableQueryable<IntentType, object>>>()))
    .ReturnsAsync(new List<IntentType>
    {
        new IntentType { Id = Guid.NewGuid(), IsActive = true }
    });

        staffIntentRepo
    .Setup(r => r.GetListAsync(
        It.IsAny<Expression<Func<StaffIntentType, bool>>>(),
        It.IsAny<Func<IQueryable<StaffIntentType>, IOrderedQueryable<StaffIntentType>>>(),
        It.IsAny<Func<IQueryable<StaffIntentType>, IIncludableQueryable<StaffIntentType, object>>>()))
    .ReturnsAsync(new List<StaffIntentType>());

        var service = CreateService();

        var result = await service.UpdateStaffAsync(staffId, new UpdateStaffRequest
        {
            StaffIntentTypes = new List<AssignStaffToIntentTypeRequest>
        {
            new AssignStaffToIntentTypeRequest { IntentId = intentId }
        }
        });

        Assert.True(result);
        staffRepo.Verify(r => r.Update(It.IsAny<Staff>()), Times.Once);
    }
}