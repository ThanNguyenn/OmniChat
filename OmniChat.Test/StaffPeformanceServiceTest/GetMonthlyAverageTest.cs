using AutoMapper;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Implements;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.StaffPeformanceServiceTest
{
    public class GetMonthlyAverageTest : IDisposable
    {
        private readonly OmniChatDbContext _dbContext;
        private readonly IUnitOfWork<OmniChatDbContext> _unitOfWork;
        private readonly StaffPerformanceService _service;

        public GetMonthlyAverageTest()
        {
            var options = new DbContextOptionsBuilder<OmniChatDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new OmniChatDbContext(options);
            _unitOfWork = new UnitOfWork<OmniChatDbContext>(_dbContext);

            _service = new StaffPerformanceService(
                _unitOfWork,
                new Mock<ILogger<StaffPerformanceService>>().Object,
                new Mock<IMapper>().Object,
                new Mock<IHttpContextAccessor>().Object
            );
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Fact]
        public async Task GetMonthlyAverageAsync_ValidYear_ReturnsCorrectStatistics()
        {
            // Arrange
            var year = 2024;

            // ── Timestamps cho CustomerMessage ──
            var jan1Ts = new DateTimeOffset(new DateTime(year, 1, 10, 0, 0, 0, DateTimeKind.Utc)).ToUnixTimeMilliseconds();
            var jan2Ts = new DateTimeOffset(new DateTime(year, 1, 20, 0, 0, 0, DateTimeKind.Utc)).ToUnixTimeMilliseconds();
            var mar1Ts = new DateTimeOffset(new DateTime(year, 3, 5, 0, 0, 0, DateTimeKind.Utc)).ToUnixTimeMilliseconds();

            // ── DateTime cho SupportConversation (FirstResponseAt) ──
            var janCreated = new DateTime(year, 1, 15, 10, 0, 0, DateTimeKind.Utc);
            var marCreated = new DateTime(year, 3, 10, 8, 0, 0, DateTimeKind.Utc);

            // ── DateTime cho SupportConversation (CloseAt) ──
            var janConvCreated = new DateTime(year, 1, 20, 0, 0, 0, DateTimeKind.Utc);
            var janConvClosed = new DateTime(year, 1, 20, 0, 30, 0, DateTimeKind.Utc); // 1800s
            var aprConvCreated = new DateTime(year, 4, 1, 0, 0, 0, DateTimeKind.Utc);
            var aprConvClosed = new DateTime(year, 4, 1, 0, 15, 0, DateTimeKind.Utc); // 900s

            // ── DateTime cho SupportTask ──
            var janTaskCreated = new DateTime(year, 1, 5, 0, 0, 0, DateTimeKind.Utc);
            var janTaskComplete = new DateTime(year, 1, 5, 1, 0, 0, DateTimeKind.Utc); // 3600s
            var febTaskCreated = new DateTime(year, 2, 10, 0, 0, 0, DateTimeKind.Utc);
            var febTaskComplete = new DateTime(year, 2, 10, 2, 0, 0, DateTimeKind.Utc); // 7200s

            // ── Seed CustomerMessage ──
            _dbContext.CustomerMessages.AddRange(
                new CustomerMessage
                {
                    Id = Guid.NewGuid(),
                    Content = "test",
                    Timestamp = jan1Ts,
                    ConversationId = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid()
                },
                new CustomerMessage
                {
                    Id = Guid.NewGuid(),
                    Content = "test",
                    Timestamp = jan2Ts,
                    ConversationId = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid()
                },
                new CustomerMessage
                {
                    Id = Guid.NewGuid(),
                    Content = "test",
                    Timestamp = mar1Ts,
                    ConversationId = Guid.NewGuid(),
                    CustomerId = Guid.NewGuid()
                }
            );

            // ── Seed SupportConversation ──
            _dbContext.SupportConversations.AddRange(
                // Jan: có FirstResponseAt, 60s
                new SupportConversation
                {
                    Id = Guid.NewGuid(),
                    CustomerName = "Test",
                    CreatedDate = janCreated,
                    FirstResponseAt = janCreated.AddSeconds(60),
                    Status = ConversationStatus.Waiting,
                    ActiveCustomerId = Guid.NewGuid(),
                    ProvidersId = Guid.NewGuid()
                },
                // Mar: có FirstResponseAt, 120s
                new SupportConversation
                {
                    Id = Guid.NewGuid(),
                    CustomerName = "Test",
                    CreatedDate = marCreated,
                    FirstResponseAt = marCreated.AddSeconds(120),
                    Status = ConversationStatus.Waiting,
                    ActiveCustomerId = Guid.NewGuid(),
                    ProvidersId = Guid.NewGuid()
                },
                // Jan: đã đóng, 1800s
                new SupportConversation
                {
                    Id = Guid.NewGuid(),
                    CustomerName = "Test",
                    CreatedDate = janConvCreated,
                    CloseAt = janConvClosed,
                    Status = ConversationStatus.Complete,
                    ActiveCustomerId = Guid.NewGuid(),
                    ProvidersId = Guid.NewGuid()
                },
                // Apr: đã đóng, 900s
                new SupportConversation
                {
                    Id = Guid.NewGuid(),
                    CustomerName = "Test",
                    CreatedDate = aprConvCreated,
                    CloseAt = aprConvClosed,
                    Status = ConversationStatus.Complete,
                    ActiveCustomerId = Guid.NewGuid(),
                    ProvidersId = Guid.NewGuid()
                }
            );

            // ── Seed SupportTask ──
            _dbContext.SupportTasks.AddRange(
                // Jan: Done, 3600s
                new SupportTask
                {
                    Id = Guid.NewGuid(),
                    Status = SupportTaskStatus.Done,
                    CreatedAt = janTaskCreated,
                    CompleteDate = janTaskComplete,
                    SupportConversationId = Guid.NewGuid(),
                    IntentTypeId = Guid.NewGuid()
                },
                // Feb: Done, 7200s
                new SupportTask
                {
                    Id = Guid.NewGuid(),
                    Status = SupportTaskStatus.Done,
                    CreatedAt = febTaskCreated,
                    CompleteDate = febTaskComplete,
                    SupportConversationId = Guid.NewGuid(),
                    IntentTypeId = Guid.NewGuid()
                }
            );

            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetMonthlyAverageAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(year, result.Year);
            Assert.Equal(12, result.MonthlyData.Count());

            var jan = result.MonthlyData.First(m => m.Month == 1);
            Assert.Equal(2, jan.TotalCustomerMessages);
            Assert.Equal(60.00, jan.AverageTotalResponseTime);
            Assert.Equal(3600.00, jan.TotalAverageTaskComplete);
            Assert.Equal(1800.00, jan.TotalAverageCompleteConversation);

            var feb = result.MonthlyData.First(m => m.Month == 2);
            Assert.Equal(0, feb.TotalCustomerMessages);
            Assert.Equal(0.00, feb.AverageTotalResponseTime);
            Assert.Equal(7200.00, feb.TotalAverageTaskComplete);
            Assert.Equal(0.00, feb.TotalAverageCompleteConversation);

            var mar = result.MonthlyData.First(m => m.Month == 3);
            Assert.Equal(1, mar.TotalCustomerMessages);
            Assert.Equal(120.00, mar.AverageTotalResponseTime);
            Assert.Equal(0.00, mar.TotalAverageTaskComplete);
            Assert.Equal(0.00, mar.TotalAverageCompleteConversation);

            var apr = result.MonthlyData.First(m => m.Month == 4);
            Assert.Equal(0, apr.TotalCustomerMessages);
            Assert.Equal(0.00, apr.AverageTotalResponseTime);
            Assert.Equal(0.00, apr.TotalAverageTaskComplete);
            Assert.Equal(900.00, apr.TotalAverageCompleteConversation);

            foreach (var month in result.MonthlyData.Where(m => m.Month > 4))
            {
                Assert.Equal(0, month.TotalCustomerMessages);
                Assert.Equal(0.00, month.AverageTotalResponseTime);
                Assert.Equal(0.00, month.TotalAverageTaskComplete);
                Assert.Equal(0.00, month.TotalAverageCompleteConversation);
            }
        }
    }
}
