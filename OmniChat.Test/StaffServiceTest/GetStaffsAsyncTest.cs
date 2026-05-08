using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.IntentType;
using OmniChat.Infrastructure.Dtos.Responses.Staff;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Claim = System.Security.Claims.Claim;

namespace OmniChat.Test.StaffServiceTest;

public class GetStaffsAsyncTest
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

    private void SetupHttpContext(string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "mock");
        var principal = new ClaimsPrincipal(identity);

        var context = new DefaultHttpContext
        {
            User = principal
        };

        _httpMock.Setup(x => x.HttpContext)
            .Returns(context);
    }

    private PagingResponse<GetStaffsResponse> CreatePagingResponse()
    {
        return new PagingResponse<GetStaffsResponse>
        {
            Items = new List<GetStaffsResponse>
            {
                new GetStaffsResponse
                {
                    Id = Guid.NewGuid(),
                    Name = "John",
                    Email = "john@mail.com",
                    Phone = "123",
                    RoleName = "Staff",
                    StaffIntentTypes = new List<GetStaffIntentTypeResponse>()
                }
            },
            Meta = new PaginationMeta
            {
                CurrentPage = 1,
                PageSize = 20,
                TotalItems = 1,
                TotalPages = 1
            }
        };
    }

    [Fact]
    public async Task GetStaffsAsync_ShouldReturnPagingResponse_WhenAdmin()
    {
        SetupHttpContext("Admin");

        var repo = SetupRepository<Staff>();

        var expected = CreatePagingResponse();

        repo.Setup(r => r.GetPagingListAsync<GetStaffsResponse>(
                It.IsAny<Expression<Func<Staff, GetStaffsResponse>>>(),
                It.IsAny<Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
                It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(expected);

        var service = CreateService();

        var result = await service.GetStaffsAsync();

        Assert.NotNull(result);
        Assert.Single(result.Items);

        repo.Verify(r => r.GetPagingListAsync<GetStaffsResponse>(
            It.IsAny<Expression<Func<Staff, GetStaffsResponse>>>(),
            It.IsAny<Expression<Func<Staff, bool>>>(),
            It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
            It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>(),
            1,
            20),
            Times.Once);
    }

    [Fact]
    public async Task GetStaffsAsync_ShouldUseNonAdminBranch_WhenUserIsStaff()
    {
        SetupHttpContext("Staff");

        var repo = SetupRepository<Staff>();

        Expression<Func<Staff, bool>> capturedPredicate = null!;

        repo.Setup(r => r.GetPagingListAsync<GetStaffsResponse>(
                It.IsAny<Expression<Func<Staff, GetStaffsResponse>>>(),
                It.IsAny<Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
                It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .Callback((
                Expression<Func<Staff, GetStaffsResponse>> selector,
                Expression<Func<Staff, bool>> predicate,
                Func<IQueryable<Staff>, IOrderedQueryable<Staff>> orderBy,
                Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>> include,
                int page,
                int size) =>
            {
                capturedPredicate = predicate;
            })
            .ReturnsAsync(CreatePagingResponse());

        var service = CreateService();

        await service.GetStaffsAsync();

        var predicate = capturedPredicate.Compile();

        var validStaff = new Staff
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            Account = new Account
            {
                Role = new Role
                {
                    Name = "Staff"
                }
            }
        };

        var manager = new Staff
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            Account = new Account
            {
                Role = new Role
                {
                    Name = "Manager"
                }
            }
        };

        Assert.True(predicate(validStaff));
        Assert.False(predicate(manager));
    }

    [Fact]
    public async Task GetStaffsAsync_ShouldPassPagingParameters()
    {
        SetupHttpContext("Admin");

        var repo = SetupRepository<Staff>();

        repo.Setup(r => r.GetPagingListAsync<GetStaffsResponse>(
                It.IsAny<Expression<Func<Staff, GetStaffsResponse>>>(),
                It.IsAny<Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
                It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(CreatePagingResponse());

        var service = CreateService();

        await service.GetStaffsAsync(
            pageNumber: 3,
            pageSize: 50);

        repo.Verify(r => r.GetPagingListAsync<GetStaffsResponse>(
            It.IsAny<Expression<Func<Staff, GetStaffsResponse>>>(),
            It.IsAny<Expression<Func<Staff, bool>>>(),
            It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
            It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>(),
            3,
            50),
            Times.Once);
    }

    [Fact]
    public async Task GetStaffsAsync_ShouldApplySearchFilter()
    {
        SetupHttpContext("Admin");

        var repo = SetupRepository<Staff>();

        Expression<Func<Staff, bool>> capturedPredicate = null!;

        repo.Setup(r => r.GetPagingListAsync<GetStaffsResponse>(
                It.IsAny<Expression<Func<Staff, GetStaffsResponse>>>(),
                It.IsAny<Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
                It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .Callback((
                Expression<Func<Staff, GetStaffsResponse>> selector,
                Expression<Func<Staff, bool>> predicate,
                Func<IQueryable<Staff>, IOrderedQueryable<Staff>> orderBy,
                Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>> include,
                int page,
                int size) =>
            {
                capturedPredicate = predicate;
            })
            .ReturnsAsync(CreatePagingResponse());

        var service = CreateService();

        await service.GetStaffsAsync(search: "john");

        var predicate = capturedPredicate.Compile();

        var matchingStaff = new Staff
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            Name = "john doe",
            Email = "abc@mail.com",
            Phone = "123",
            Account = new Account
            {
                Role = new Role
                {
                    Name = "Staff"
                }
            },
            StaffIntentTypes = new List<StaffIntentType>()
        };

        var nonMatchingStaff = new Staff
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            Name = "mike",
            Email = "mike@mail.com",
            Phone = "999",
            Account = new Account
            {
                Role = new Role
                {
                    Name = "Staff"
                }
            },
            StaffIntentTypes = new List<StaffIntentType>()
        };

        Assert.True(predicate(matchingStaff));
        Assert.False(predicate(nonMatchingStaff));
    }

    [Fact]
    public async Task GetStaffsAsync_ShouldApplyDepartmentFilter()
    {
        SetupHttpContext("Admin");

        var repo = SetupRepository<Staff>();

        Expression<Func<Staff, bool>> capturedPredicate = null!;

        repo.Setup(r => r.GetPagingListAsync<GetStaffsResponse>(
                It.IsAny<Expression<Func<Staff, GetStaffsResponse>>>(),
                It.IsAny<Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
                It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .Callback((
                Expression<Func<Staff, GetStaffsResponse>> selector,
                Expression<Func<Staff, bool>> predicate,
                Func<IQueryable<Staff>, IOrderedQueryable<Staff>> orderBy,
                Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>> include,
                int page,
                int size) =>
            {
                capturedPredicate = predicate;
            })
            .ReturnsAsync(CreatePagingResponse());

        var departmentId = Guid.NewGuid();

        var service = CreateService();

        await service.GetStaffsAsync(
            departmentIds: new List<Guid> { departmentId });

        var predicate = capturedPredicate.Compile();

        var validStaff = new Staff
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            Account = new Account
            {
                Role = new Role
                {
                    Name = "Staff"
                }
            },
            StaffIntentTypes = new List<StaffIntentType>
            {
                new StaffIntentType
                {
                    IntentTypeId = departmentId
                }
            }
        };

        var invalidStaff = new Staff
        {
            Id = Guid.NewGuid(),
            IsActive = true,
            Account = new Account
            {
                Role = new Role
                {
                    Name = "Staff"
                }
            },
            StaffIntentTypes = new List<StaffIntentType>()
        };

        Assert.True(predicate(validStaff));
        Assert.False(predicate(invalidStaff));
    }
}
