using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Invoice;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.InvoiceServiceTest;

public class GetInvoicesAsyncTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<InvoiceService>> _loggerMock = new();
    protected readonly Mock<IMailService> _mailServiceMock = new();

    public InvoiceService CreateService()
    {
        return new InvoiceService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _mailServiceMock.Object
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
    public async Task GetInvoicesAsync_ShouldFilterByCustomerId()
    {
        var repo = SetupRepository<Invoice>();

        var customerId = Guid.NewGuid();

        var invoices = new List<Invoice>
        {
            new Invoice
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                InvoiceStatus = InvoiceStatus.Pending,
                CustomerProfile = new CustomerProfile
                {
                    CustomerName = "John Doe"
                }
            },
            new Invoice
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                InvoiceStatus = InvoiceStatus.Pending,
                CustomerProfile = new CustomerProfile
                {
                    CustomerName = "Other"
                }
            }
        };

        repo.Setup(r => r.GetPagingListAsync<GetInvoicesResponse>(
         It.IsAny<Expression<Func<Invoice, GetInvoicesResponse>>>(),
         It.IsAny<Expression<Func<Invoice, bool>>>(),
         It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
         It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>(),
         It.IsAny<int>(),
         It.IsAny<int>()))
     .Returns((
         Expression<Func<Invoice, GetInvoicesResponse>> selector,
         Expression<Func<Invoice, bool>> predicate,
         Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>> orderBy,
         Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>> include,
         int page,
         int size) =>
     {
         IQueryable<Invoice> query = new TestAsyncEnumerable<Invoice>(invoices);

         if (include != null)
             query = include(query);

         if (predicate != null)
             query = query.Where(predicate);

         if (orderBy != null)
             query = orderBy(query);

         var projected = query.AsQueryable().Select(selector).ToList();

         return Task.FromResult(new PagingResponse<GetInvoicesResponse>
         {
             Items = projected,
             Meta = new PaginationMeta
             {
                 CurrentPage = page,
                 PageSize = size,
                 TotalItems = projected.Count,
                 TotalPages = 1
             }
         });
     });

        var service = CreateService();

        var result = await service.GetInvoicesAsync(customerId, null, null);

        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetInvoicesAsync_ShouldFilterByStatus()
    {
        var repo = SetupRepository<Invoice>();

        var invoices = new List<Invoice>
        {
            new Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceStatus = InvoiceStatus.Completed
            },
            new Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceStatus = InvoiceStatus.Pending
            }
        };

        repo.Setup(r => r.GetPagingListAsync<GetInvoicesResponse>(
        It.IsAny<Expression<Func<Invoice, GetInvoicesResponse>>>(),
        It.IsAny<Expression<Func<Invoice, bool>>>(),
        It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
        It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>(),
        It.IsAny<int>(),
        It.IsAny<int>()))
    .Returns((
        Expression<Func<Invoice, GetInvoicesResponse>> selector,
        Expression<Func<Invoice, bool>> predicate,
        Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>> orderBy,
        Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>> include,
        int page,
        int size) =>
    {
        IQueryable<Invoice> query = new TestAsyncEnumerable<Invoice>(invoices);

        if (include != null)
            query = include(query);

        if (predicate != null)
            query = query.Where(predicate);

        if (orderBy != null)
            query = orderBy(query);

        var projected = query.AsQueryable().Select(selector).ToList();

        return Task.FromResult(new PagingResponse<GetInvoicesResponse>
        {
            Items = projected,
            Meta = new PaginationMeta
            {
                CurrentPage = page,
                PageSize = size,
                TotalItems = projected.Count,
                TotalPages = 1
            }
        });
    });

        var service = CreateService();

        var result = await service.GetInvoicesAsync(null, null, InvoiceStatus.Completed);

        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetInvoicesAsync_ShouldFilterByCustomerName()
    {
        var repo = SetupRepository<Invoice>();

        var invoices = new List<Invoice>
        {
            new Invoice
            {
                Id = Guid.NewGuid(),
                CustomerProfile = new CustomerProfile
                {
                    CustomerName = "John Doe"
                }
            },
            new Invoice
            {
                Id = Guid.NewGuid(),
                CustomerProfile = new CustomerProfile
                {
                    CustomerName = "Alice"
                }
            }
        };

        repo.Setup(r => r.GetPagingListAsync<GetInvoicesResponse>(
         It.IsAny<Expression<Func<Invoice, GetInvoicesResponse>>>(),
         It.IsAny<Expression<Func<Invoice, bool>>>(),
         It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
         It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>(),
         It.IsAny<int>(),
         It.IsAny<int>()))
     .Returns((
         Expression<Func<Invoice, GetInvoicesResponse>> selector,
         Expression<Func<Invoice, bool>> predicate,
         Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>> orderBy,
         Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>> include,
         int page,
         int size) =>
     {
         IQueryable<Invoice> query = new TestAsyncEnumerable<Invoice>(invoices);

         if (include != null)
             query = include(query);

         if (predicate != null)
             query = query.Where(predicate);

         if (orderBy != null)
             query = orderBy(query);

         var projected = query.AsQueryable().Select(selector).ToList();

         return Task.FromResult(new PagingResponse<GetInvoicesResponse>
         {
             Items = projected,
             Meta = new PaginationMeta
             {
                 CurrentPage = page,
                 PageSize = size,
                 TotalItems = projected.Count,
                 TotalPages = 1
             }
         });
     });

        var service = CreateService();

        var result = await service.GetInvoicesAsync(null, "john", null);

        Assert.Single(result.Items);
    }
}