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

public class AssignIntentToStaffAsyncTest
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
    public async Task AssignIntentToStaffAsync_ShouldInsertNewAssignments_WhenValid()
    {
        var staffIntentRepo = SetupRepository<StaffIntentType>();
        var intentRepo = SetupRepository<IntentType>();

        var staffId = Guid.NewGuid();
        var intentId = Guid.NewGuid();

        intentRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<IntentType, bool>>>(),
                It.IsAny<Func<IQueryable<IntentType>, IOrderedQueryable<IntentType>>>(),
                It.IsAny<Func<IQueryable<IntentType>, IIncludableQueryable<IntentType, object>>>()))
            .ReturnsAsync(new List<IntentType>
            {
            new IntentType
            {
                Id = intentId,
                IsActive = true
            }
            });

        staffIntentRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<StaffIntentType, bool>>>(),
                It.IsAny<Func<IQueryable<StaffIntentType>, IOrderedQueryable<StaffIntentType>>>(),
                It.IsAny<Func<IQueryable<StaffIntentType>, IIncludableQueryable<StaffIntentType, object>>>()))
            .ReturnsAsync(new List<StaffIntentType>());

        _uowMock.Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        var result = await service.AssignIntentToStaffAsync(
            staffId,
            new List<AssignStaffToIntentTypeRequest>
            {
            new AssignStaffToIntentTypeRequest
            {
                IntentId = intentId
            }
            });

        Assert.True(result);

        staffIntentRepo.Verify(r => r.InsertRangeAsync(
            It.Is<List<StaffIntentType>>(x =>
                x.Count == 1 &&
                x.First().StaffId == staffId &&
                x.First().IntentTypeId == intentId)),
            Times.Once);
    }

    [Fact]
    public async Task AssignIntentToStaffAsync_ShouldThrowNotFoundException_WhenIntentNotExists()
    {
        var staffIntentRepo = SetupRepository<StaffIntentType>();
        var intentRepo = SetupRepository<IntentType>();

        var intentId1 = Guid.NewGuid();
        var intentId2 = Guid.NewGuid();

        intentRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<IntentType, bool>>>(),
                It.IsAny<Func<IQueryable<IntentType>, IOrderedQueryable<IntentType>>>(),
                It.IsAny<Func<IQueryable<IntentType>, IIncludableQueryable<IntentType, object>>>()))
            .ReturnsAsync(new List<IntentType>
            {
            new IntentType
            {
                Id = intentId1,
                IsActive = true
            }
            });

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.AssignIntentToStaffAsync(
                Guid.NewGuid(),
                new List<AssignStaffToIntentTypeRequest>
                {
                new AssignStaffToIntentTypeRequest
                {
                    IntentId = intentId1
                },
                new AssignStaffToIntentTypeRequest
                {
                    IntentId = intentId2
                }
                }));
    }

    [Fact]
    public async Task AssignIntentToStaffAsync_ShouldSkipExistingAssignments()
    {
        var staffIntentRepo = SetupRepository<StaffIntentType>();
        var intentRepo = SetupRepository<IntentType>();

        var staffId = Guid.NewGuid();
        var intentId = Guid.NewGuid();

        intentRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<IntentType, bool>>>(),
                It.IsAny<Func<IQueryable<IntentType>, IOrderedQueryable<IntentType>>>(),
                It.IsAny<Func<IQueryable<IntentType>, IIncludableQueryable<IntentType, object>>>()))
            .ReturnsAsync(new List<IntentType>
            {
            new IntentType
            {
                Id = intentId,
                IsActive = true
            }
            });

        staffIntentRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<StaffIntentType, bool>>>(),
                It.IsAny<Func<IQueryable<StaffIntentType>, IOrderedQueryable<StaffIntentType>>>(),
                It.IsAny<Func<IQueryable<StaffIntentType>, IIncludableQueryable<StaffIntentType, object>>>()))
            .ReturnsAsync(new List<StaffIntentType>
            {
            new StaffIntentType
            {
                StaffId = staffId,
                IntentTypeId = intentId
            }
            });

        var service = CreateService();

        var result = await service.AssignIntentToStaffAsync(
            staffId,
            new List<AssignStaffToIntentTypeRequest>
            {
            new AssignStaffToIntentTypeRequest
            {
                IntentId = intentId
            }
            });

        Assert.True(result);

        staffIntentRepo.Verify(r =>
            r.InsertRangeAsync(It.IsAny<List<StaffIntentType>>()),
            Times.Never);
    }

    [Fact]
    public async Task AssignIntentToStaffAsync_ShouldDeduplicateIntentIds()
    {
        var staffIntentRepo = SetupRepository<StaffIntentType>();
        var intentRepo = SetupRepository<IntentType>();

        var staffId = Guid.NewGuid();
        var intentId = Guid.NewGuid();

        intentRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<IntentType, bool>>>(),
                It.IsAny<Func<IQueryable<IntentType>, IOrderedQueryable<IntentType>>>(),
                It.IsAny<Func<IQueryable<IntentType>, IIncludableQueryable<IntentType, object>>>()))
            .ReturnsAsync(new List<IntentType>
            {
            new IntentType
            {
                Id = intentId,
                IsActive = true
            }
            });

        staffIntentRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<StaffIntentType, bool>>>(),
                It.IsAny<Func<IQueryable<StaffIntentType>, IOrderedQueryable<StaffIntentType>>>(),
                It.IsAny<Func<IQueryable<StaffIntentType>, IIncludableQueryable<StaffIntentType, object>>>()))
            .ReturnsAsync(new List<StaffIntentType>());

        _uowMock.Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        var service = CreateService();

        await service.AssignIntentToStaffAsync(
            staffId,
            new List<AssignStaffToIntentTypeRequest>
            {
            new AssignStaffToIntentTypeRequest
            {
                IntentId = intentId
            },
            new AssignStaffToIntentTypeRequest
            {
                IntentId = intentId
            }
            });

        staffIntentRepo.Verify(r => r.InsertRangeAsync(
            It.Is<List<StaffIntentType>>(x => x.Count == 1)),
            Times.Once);
    }

    [Fact]
    public async Task AssignIntentToStaffAsync_ShouldNotCallTransaction_WhenNothingToInsert()
    {
        var staffIntentRepo = SetupRepository<StaffIntentType>();
        var intentRepo = SetupRepository<IntentType>();

        var staffId = Guid.NewGuid();
        var intentId = Guid.NewGuid();

        intentRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<IntentType, bool>>>(),
                It.IsAny<Func<IQueryable<IntentType>, IOrderedQueryable<IntentType>>>(),
                It.IsAny<Func<IQueryable<IntentType>, IIncludableQueryable<IntentType, object>>>()))
            .ReturnsAsync(new List<IntentType>
            {
            new IntentType
            {
                Id = intentId,
                IsActive = true
            }
            });

        staffIntentRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<StaffIntentType, bool>>>(),
                It.IsAny<Func<IQueryable<StaffIntentType>, IOrderedQueryable<StaffIntentType>>>(),
                It.IsAny<Func<IQueryable<StaffIntentType>, IIncludableQueryable<StaffIntentType, object>>>()))
            .ReturnsAsync(new List<StaffIntentType>
            {
            new StaffIntentType
            {
                StaffId = staffId,
                IntentTypeId = intentId
            }
            });

        var service = CreateService();

        await service.AssignIntentToStaffAsync(
            staffId,
            new List<AssignStaffToIntentTypeRequest>
            {
            new AssignStaffToIntentTypeRequest
            {
                IntentId = intentId
            }
            });

        _uowMock.Verify(x =>
            x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()),
            Times.Never);
    }


}
