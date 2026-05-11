using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.InvoiceServiceTest;

public class GetTotalUnpaidAsyncTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<InvoiceService>> _loggerMock = new();

    public InvoiceService CreateService()
    {
        return new InvoiceService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object
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
    public async Task GetTotalUnpaidAsync_ShouldReturn12Months_WhenInputIsYear()
    {
        var invoiceRepo = SetupRepository<Invoice>();

        var invoices = new List<Invoice>
        {
            new Invoice
            {
                CreateAt = new DateTime(2025, 1, 10),
                InvoiceStatus = InvoiceStatus.Pending,
                Total = 1000,
                PaidAmount = 200,
                DeductedAmount = 100
            },
            new Invoice
            {
                CreateAt = new DateTime(2025, 2, 10),
                InvoiceStatus = InvoiceStatus.PartialPaid,
                Total = 2000,
                PaidAmount = 500,
                DeductedAmount = 200
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

        var result = (await service.GetTotalUnpaidAsync("2025")).ToList();

        Assert.Equal(12, result.Count);

        Assert.Contains(result, x => x.Month == "01/2025" && x.TotalAmount == 700);
        Assert.Contains(result, x => x.Month == "02/2025" && x.TotalAmount == 1300);
    }

    [Fact]
    public async Task GetTotalUnpaidAsync_ShouldReturnSingleMonth_WhenInputIsMonthYear()
    {
        var invoiceRepo = SetupRepository<Invoice>();

        var invoices = new List<Invoice>
    {
        new Invoice
        {
            CreateAt = new DateTime(2025, 5, 15),
            InvoiceStatus = InvoiceStatus.Pending,
            Total = 1000,
            PaidAmount = 300,
            DeductedAmount = 100
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

        var result = (await service.GetTotalUnpaidAsync("05/2025")).ToList();

        Assert.Single(result);

        Assert.Equal("05/2025", result[0].Month);

        Assert.Equal(600, result[0].TotalAmount);
    }


    [Fact]
    public async Task GetTotalUnpaidAsync_ShouldIgnorePaidInvoices()
    {
        var invoiceRepo = SetupRepository<Invoice>();

        var invoices = new List<Invoice>
        {
            new Invoice
            {
                CreateAt = new DateTime(2025, 5, 15),
                InvoiceStatus = InvoiceStatus.Completed,
                Total = 1000,
                PaidAmount = 1000,
                DeductedAmount = 0
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

        var result = (await service.GetTotalUnpaidAsync("05/2025")).ToList();

        Assert.Single(result);
        Assert.Equal(0, result[0].TotalAmount);
    }
}