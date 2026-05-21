using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.InvoiceServiceTest;

public class GetTotalIncomeAsyncTest
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

    protected Mock<IGenericRepository<T>> SetupRepository<T>()
        where T : class
    {
        var repoMock = new Mock<IGenericRepository<T>>();

        _uowMock.Setup(x => x.GetRepository<T>())
            .Returns(repoMock.Object);

        return repoMock;
    }

    [Fact]
    public async Task GetTotalIncomeAsync_ShouldReturn12Months_WhenInputIsYear()
    {
        var invoiceRepo = SetupRepository<Invoice>();

        var invoices = new List<Invoice>
    {
        new Invoice
        {
            CompletedDate = new DateTime(2025, 1, 10),
            InvoiceStatus = InvoiceStatus.Completed,
            PaidAmount = 100
        },
        new Invoice
        {
            CompletedDate = new DateTime(2025, 2, 10),
            InvoiceStatus = InvoiceStatus.Completed,
            PaidAmount = 200
        }
    };

        invoiceRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IQueryable<Invoice>>>(),
                true))
            .Returns((Expression<Func<Invoice, bool>> predicate,
                      Func<IQueryable<Invoice>, IQueryable<Invoice>> include,
                      bool asNoTracking) =>
            {
                IQueryable<Invoice> query = new TestAsyncEnumerable<Invoice>(invoices);

                if (predicate != null)
                    query = query.Where(predicate);

                return new TestAsyncEnumerable<Invoice>(query);
            });

        var service = CreateService();

        var result = (await service.GetTotalIncomeAsync("2025")).ToList();

        Assert.Equal(12, result.Count);

        Assert.Contains(result, x => x.Month == "01/2025" && x.TotalAmount == 100);
        Assert.Contains(result, x => x.Month == "02/2025" && x.TotalAmount == 200);
    }

    [Fact]
    public async Task GetTotalIncomeAsync_ShouldReturnSingleMonth_WhenInputIsMonthYear()
    {
        var invoiceRepo = SetupRepository<Invoice>();

        var invoices = new List<Invoice>
    {
        new Invoice
        {
            CompletedDate = new DateTime(2025, 5, 15),
            InvoiceStatus = InvoiceStatus.Completed,
            PaidAmount = 500
        }
    };

        invoiceRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IQueryable<Invoice>>>(),
                true))
            .Returns((Expression<Func<Invoice, bool>> predicate,
                      Func<IQueryable<Invoice>, IQueryable<Invoice>> include,
                      bool asNoTracking) =>
            {
                IQueryable<Invoice> query = new TestAsyncEnumerable<Invoice>(invoices);

                if (predicate != null)
                    query = query.Where(predicate);

                return new TestAsyncEnumerable<Invoice>(query);
            });

        var service = CreateService();

        var result = (await service.GetTotalIncomeAsync("05/2025")).ToList();

        Assert.Single(result);

        Assert.Equal("05/2025", result[0].Month);
        Assert.Equal(500, result[0].TotalAmount);
    }

    [Fact]
    public async Task GetTotalIncomeAsync_ShouldIgnoreNonCompletedInvoices()
    {
        var invoiceRepo = SetupRepository<Invoice>();

        var invoices = new List<Invoice>
        {
            new Invoice
            {
                CompletedDate = new DateTime(2025, 5, 15),
                InvoiceStatus = InvoiceStatus.Pending,
                Total = 500
            }
        };

        invoiceRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IQueryable<Invoice>>>(),
                true))
            .Returns((
                Expression<Func<Invoice, bool>> predicate,
                Func<IQueryable<Invoice>, IQueryable<Invoice>> include,
                bool asNoTracking) =>
            {
                IQueryable<Invoice> query =
                    new TestAsyncEnumerable<Invoice>(invoices);

                if (predicate != null)
                    query = query.Where(predicate);

                if (include != null)
                    query = include(query);

                return query;
            });

        var service = CreateService();

        var result = (await service.GetTotalIncomeAsync("05/2025")).ToList();

        Assert.Single(result);

        Assert.Equal(0, result[0].TotalAmount);
    }
}