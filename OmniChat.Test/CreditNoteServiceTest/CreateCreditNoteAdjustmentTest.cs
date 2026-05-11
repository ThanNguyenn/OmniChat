using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.CreditNoteServiceTest;

public class CreateCreditNoteAdjustmentTest
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    private readonly Mock<ILogger<CreditNoteService>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IHttpContextAccessor> _httpMock = new();
    private readonly Mock<IInvoiceService> _invoiceServiceMock = new();

    private CreditNoteService CreateService()
    {
        return new CreditNoteService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _invoiceServiceMock.Object
        );
    }

    private Mock<IGenericRepository<T>> SetupRepository<T>() where T : class
    {
        var repo = new Mock<IGenericRepository<T>>();

        _uowMock.Setup(x => x.GetRepository<T>())
            .Returns(repo.Object);

        return repo;
    }

    private void SetupTransaction()
    {
        _uowMock
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
            .Returns<Func<Task<bool>>>(f => f());

        _uowMock
            .Setup(x => x.CommitAsync())
            .ReturnsAsync(1);
    }

    [Fact]
    public async Task CreateCreditNoteAdjustment_ShouldThrow_WhenAmountIsZeroOrNegative()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.CreateCreditNoteAdjustmentAsync(Guid.NewGuid(), 0));
    }

    [Fact]
    public async Task CreateCreditNoteAdjustment_ShouldInsertCreditNote_WhenValid()
    {
        var repo = SetupRepository<CreditNote>();
        SetupTransaction();

        var service = CreateService();

        var orderId = Guid.NewGuid();
        var amount = 100;

        var result = await service.CreateCreditNoteAdjustmentAsync(orderId, amount);

        Assert.True(result);

        repo.Verify(r => r.InsertAsync(It.Is<CreditNote>(c =>
            c.OrderId == orderId &&
            c.Total == amount &&
            c.CreditNoteType == CreditNoteType.Adjustment &&
            c.CreditNoteStatus == CreditNoteStatus.Pending)),
            Times.Once);
    }
}