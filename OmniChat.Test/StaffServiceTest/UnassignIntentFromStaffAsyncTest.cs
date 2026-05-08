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

public class UnassignIntentFromStaffAsyncTest
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

    protected Mock<IGenericRepository<T>> SetupRepository<T>() where T : class
    {
        var repoMock = new Mock<IGenericRepository<T>>();

        _uowMock.Setup(x => x.GetRepository<T>())
            .Returns(repoMock.Object);

        return repoMock;
    }

    [Fact]
    public async Task UnassignIntentFromStaffAsync_ShouldDeleteAssignment_WhenValid()
    {
        var staffRepo = SetupRepository<Staff>();
        var staffIntentRepo = SetupRepository<StaffIntentType>();

        var staffId = Guid.NewGuid();
        var intentId = Guid.NewGuid();

        var staff = new Staff
        {
            Id = staffId,
            IsActive = true
        };

        var assignment = new StaffIntentType
        {
            StaffId = staffId,
            IntentTypeId = intentId
        };

        staffRepo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Staff, bool>>>(),
            It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
            It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
        .ReturnsAsync(staff);

        staffIntentRepo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<StaffIntentType, bool>>>(),
            It.IsAny<Func<IQueryable<StaffIntentType>, IOrderedQueryable<StaffIntentType>>>(),
            It.IsAny<Func<IQueryable<StaffIntentType>, IIncludableQueryable<StaffIntentType, object>>>()))
        .ReturnsAsync(assignment);

        _uowMock.Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        var result = await service.UnassignIntentFromStaffAsync(
            staffId,
            new AssignStaffToIntentTypeRequest
            {
                IntentId = intentId
            });

        Assert.True(result);

        staffIntentRepo.Verify(r =>
            r.Delete(It.Is<StaffIntentType>(x =>
                x.StaffId == staffId &&
                x.IntentTypeId == intentId)),
            Times.Once);
    }

    [Fact]
    public async Task UnassignIntentFromStaffAsync_ShouldThrowNotFoundException_WhenStaffNotExists()
    {
        var staffRepo = SetupRepository<Staff>();
        var staffIntentRepo = SetupRepository<StaffIntentType>();

        staffRepo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Staff, bool>>>(),
            It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
            It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
        .ReturnsAsync((Staff)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UnassignIntentFromStaffAsync(
                Guid.NewGuid(),
                new AssignStaffToIntentTypeRequest
                {
                    IntentId = Guid.NewGuid()
                }));
    }

    [Fact]
    public async Task UnassignIntentFromStaffAsync_ShouldThrowNotFoundException_WhenAssignmentNotExists()
    {
        var staffRepo = SetupRepository<Staff>();
        var staffIntentRepo = SetupRepository<StaffIntentType>();

        var staffId = Guid.NewGuid();

        staffRepo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Staff, bool>>>(),
            It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
            It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
        .ReturnsAsync(new Staff
        {
            Id = staffId,
            IsActive = true
        });

        staffIntentRepo.Setup(r => r.SingleOrDefaultAsync(
        It.IsAny<Expression<Func<StaffIntentType, bool>>>(),
        It.IsAny<Func<IQueryable<StaffIntentType>, IOrderedQueryable<StaffIntentType>>>(),
        It.IsAny<Func<IQueryable<StaffIntentType>, IIncludableQueryable<StaffIntentType, object>>>()))
    .ReturnsAsync((StaffIntentType)null);

        _uowMock.Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UnassignIntentFromStaffAsync(
                staffId,
                new AssignStaffToIntentTypeRequest
                {
                    IntentId = Guid.NewGuid()
                }));
    }

    [Fact]
    public async Task UnassignIntentFromStaffAsync_ShouldCallTransaction()
    {
        var staffRepo = SetupRepository<Staff>();
        var staffIntentRepo = SetupRepository<StaffIntentType>();

        var staffId = Guid.NewGuid();
        var intentId = Guid.NewGuid();

        staffRepo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<Staff, bool>>>(),
            It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
            It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()))
        .ReturnsAsync(new Staff
        {
            Id = staffId,
            IsActive = true
        });

        staffIntentRepo.Setup(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<StaffIntentType, bool>>>(),
            It.IsAny<Func<IQueryable<StaffIntentType>, IOrderedQueryable<StaffIntentType>>>(),
            It.IsAny<Func<IQueryable<StaffIntentType>, IIncludableQueryable<StaffIntentType, object>>>()))
        .ReturnsAsync(new StaffIntentType
        {
            StaffId = staffId,
            IntentTypeId = intentId
        });

        _uowMock.Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        await service.UnassignIntentFromStaffAsync(
            staffId,
            new AssignStaffToIntentTypeRequest
            {
                IntentId = intentId
            });

        _uowMock.Verify(x =>
            x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()),
            Times.Once);
    }
}
