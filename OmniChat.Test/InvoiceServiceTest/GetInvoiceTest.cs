using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.Invoice;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.InvoiceServiceTest;

public class GetInvoiceTest
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

    protected Mock<IGenericRepository<T>> SetupRepository<T>() where T : class
    {
        var repoMock = new Mock<IGenericRepository<T>>();

        _uowMock.Setup(x => x.GetRepository<T>())
            .Returns(repoMock.Object);

        return repoMock;
    }

    [Fact]
    public async Task GetInvoice_ShouldThrowNotFound_WhenInvoiceNotExists()
    {
        var invoiceRepo = SetupRepository<Invoice>();

        var invoiceId = Guid.NewGuid();

        invoiceRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>()))
            .ReturnsAsync((Invoice)null);

        var service = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetInvoiceAsync(invoiceId));

        invoiceRepo.Verify(r =>
            r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetInvoice_ShouldReturnMappedResponse_WhenInvoiceExists()
    {
        var invoiceRepo = SetupRepository<Invoice>();

        var invoiceId = Guid.NewGuid();

        var invoice = new Invoice
        {
            Id = invoiceId,
            CustomerId = Guid.NewGuid(),
            Total = 1500
        };

        var expectedResponse = new GetInvoiceResponse
        {
            Id = invoiceId,
            Total = 1500
        };

        invoiceRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>()))
            .ReturnsAsync(invoice);

        _mapperMock.Setup(m => m.Map<GetInvoiceResponse>(invoice))
            .Returns(expectedResponse);

        var service = CreateService();

        var result = await service.GetInvoiceAsync(invoiceId);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Id, result.Id);
        Assert.Equal(expectedResponse.Total, result.Total);

        invoiceRepo.Verify(r =>
            r.SingleOrDefaultAsync(
                It.Is<Expression<Func<Invoice, bool>>>(expr => true),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>()),
            Times.Once);

        _mapperMock.Verify(m =>
            m.Map<GetInvoiceResponse>(invoice),
            Times.Once);
    }

    [Fact]
    public async Task GetInvoice_ShouldIncludeCustomerProfile()
    {
        var invoiceRepo = SetupRepository<Invoice>();

        var invoiceId = Guid.NewGuid();

        var invoice = new Invoice
        {
            Id = invoiceId,
            CustomerProfile = new CustomerProfile()
        };

        invoiceRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>()))
            .ReturnsAsync(invoice);

        _mapperMock.Setup(m => m.Map<GetInvoiceResponse>(It.IsAny<Invoice>()))
            .Returns(new GetInvoiceResponse());

        var service = CreateService();

        await service.GetInvoiceAsync(invoiceId);

        invoiceRepo.Verify(r =>
            r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Invoice, bool>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IOrderedQueryable<Invoice>>>(),
                It.IsAny<Func<IQueryable<Invoice>, IIncludableQueryable<Invoice, object>>>()),
            Times.Once);
    }
}