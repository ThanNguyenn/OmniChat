using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.OrderServiceTest;

public class GetDashboardAsyncTest
{
    protected readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    protected readonly Mock<IHttpContextAccessor> _httpMock = new();
    protected readonly Mock<IMapper> _mapperMock = new();
    protected readonly Mock<ILogger<OrderService>> _loggerMock = new();
    protected readonly Mock<ICreditNoteService> _creditNoteMock = new();
    protected readonly Mock<IMailService> _mailServiceMock = new();
    protected readonly Mock<IInvoiceService> _invoiceServiceMock = new();

    public OrderService CreateService()
    {
        return new OrderService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _creditNoteMock.Object,
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

    protected void SetupTransaction()
    {
        _uowMock
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
            .Returns<Func<Task<bool>>>(f => f());

        _uowMock
            .Setup(x => x.ProcessInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(f => f());

        _uowMock.SetupGet(x => x.Context)
    .Returns(new OmniChatDbContext(
        new DbContextOptionsBuilder<OmniChatDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options));
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldReturnGroupedData_ForYear()
    {
        var orderRepo = SetupRepository<Order>();
        SetupTransaction();

        var data = new List<Order>
    {
        new Order
        {
            OrderDate = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc),
            Status = OrderStatus.Completed
        },
        new Order
        {
            OrderDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            Status = OrderStatus.Cancelled
        },
        new Order
        {
            OrderDate = new DateTime(2025, 2, 10, 0, 0, 0, DateTimeKind.Utc),
            Status = OrderStatus.Returned
        }
    }.AsQueryable();

        orderRepo.Setup(r => r.GetQueryable(
         It.IsAny<Expression<Func<Order, bool>>>(),
         It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
         It.IsAny<bool>()))
     .Returns((Expression<Func<Order, bool>> predicate,
               Func<IQueryable<Order>, IQueryable<Order>> include,
               bool asNoTracking) =>
     {
         IQueryable<Order> query = new TestAsyncEnumerable<Order>(data);

         if (predicate != null)
             query = query.Where(predicate);

         if (include != null)
             query = include(query);

         return query;
     });

        var service = CreateService();

        var result = await service.GetDashboardAsync(null, "2025");

        Assert.Equal(12, result.Count());
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldFilterByStatus()
    {
        var orderRepo = SetupRepository<Order>();
        SetupTransaction();

        var data = new List<Order>
    {
        new Order
        {
            OrderDate = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc),
            Status = OrderStatus.Completed
        },
        new Order
        {
            OrderDate = new DateTime(2025, 1, 12, 0, 0, 0, DateTimeKind.Utc),
            Status = OrderStatus.Cancelled
        }
    }.AsQueryable();
        orderRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Order, bool>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<bool>()))
            .Returns((Expression<Func<Order, bool>> predicate,
                      Func<IQueryable<Order>, IQueryable<Order>> include,
                      bool asNoTracking) =>
            {
                IQueryable<Order> query = new TestAsyncEnumerable<Order>(data);

                if (predicate != null)
                    query = query.Where(predicate);

                if (include != null)
                    query = include(query);

                return query;
            });

        var service = CreateService();

        var result = await service.GetDashboardAsync(
            new[] { "completed" },
            "2025"
        );

        var january = result.First(r => r.Month == "01/2025");

        Assert.Contains(january.Status, s => s.Status == OrderStatus.Completed);
    }

    [Fact]
    public async Task GetDashboardAsync_ShouldHandleSingleMonth()
    {
        var orderRepo = SetupRepository<Order>();
        SetupTransaction();

        var data = new List<Order>
    {
        new Order
        {
            OrderDate = new DateTime(2025, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            Status = OrderStatus.Returned
        }
    }.AsQueryable();

        orderRepo.Setup(r => r.GetQueryable(
        It.IsAny<Expression<Func<Order, bool>>>(),
        It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
        It.IsAny<bool>()))
    .Returns((Expression<Func<Order, bool>> predicate,
              Func<IQueryable<Order>, IQueryable<Order>> include,
              bool asNoTracking) =>
    {
        IQueryable<Order> query = new TestAsyncEnumerable<Order>(data);

        if (predicate != null)
            query = query.Where(predicate);

        if (include != null)
            query = include(query);

        return query;
    });
        var service = CreateService();

        var result = await service.GetDashboardAsync(null, "3/2025");

        Assert.Single(result);
        Assert.Equal("03/2025", result.First().Month);
    }
}
