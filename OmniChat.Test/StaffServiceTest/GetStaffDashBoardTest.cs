using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
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

namespace OmniChat.Test.StaffServiceTest
{
    public class GetStaffDashBoardTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<SupportTask>> _mockTaskRepo;
        private readonly Mock<IGenericRepository<Order>> _mockOrderRepo;
        private readonly Mock<IGenericRepository<StaffPerformance>> _mockPerformanceRepo;
        private readonly StaffService _service;

        public GetStaffDashBoardTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockTaskRepo = new Mock<IGenericRepository<SupportTask>>();
            _mockOrderRepo = new Mock<IGenericRepository<Order>>();
            _mockPerformanceRepo = new Mock<IGenericRepository<StaffPerformance>>();

            _mockUow.Setup(u => u.GetRepository<SupportTask>()).Returns(_mockTaskRepo.Object);
            _mockUow.Setup(u => u.GetRepository<Order>()).Returns(_mockOrderRepo.Object);
            _mockUow.Setup(u => u.GetRepository<StaffPerformance>()).Returns(_mockPerformanceRepo.Object);

            _service = new StaffService(
                _mockUow.Object,
                new Mock<ILogger<StaffService>>().Object,
                new Mock<IMapper>().Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IR2StorageService>().Object);
        }

        [Fact]
        public async Task GetStaffDassboardByIdAsync_ReturnsCorrectStats()
        {
            var staffId = Guid.NewGuid();
            var now = DateTime.UtcNow;

            var performance = new StaffPerformance
            {
                StaffId = staffId,
                TaskCompleted = 10,
                AvgTaskHandleTime = 120,
                FromTime = now.AddDays(-1),
                ToTime = now.AddDays(1)
            };

            _mockPerformanceRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<StaffPerformance, bool>>>(),
                null,
                null
            )).ReturnsAsync(performance);


            _mockOrderRepo.Setup(r => r.CountAsync(
                It.IsAny<Expression<Func<Order, bool>>>()
            )).ReturnsAsync(2); 


            var result = await _service.GetStaffDassboardByIdAsync(staffId);


            Assert.NotNull(result);
            Assert.Equal(10, result.TotalDoneTask);
            Assert.Equal(2, result.TotalCreateOrder); 
            Assert.Equal(2.0, result.AfferageResolveTime);
        }

        [Fact]
        public async Task GetStaffDassboardByIdAsync_WhenNoPerformanceData_ReturnsDefaults()
        {
         
            var staffId = Guid.NewGuid();

        
            _mockPerformanceRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<StaffPerformance, bool>>>(),
                null,
                null
            )).ReturnsAsync((StaffPerformance)null);

           
            var emptyOrders = new List<Order>().AsQueryable().BuildMock();
            _mockOrderRepo.Setup(r => r.GetQueryable(null, null, false)).Returns(emptyOrders);

            var result = await _service.GetStaffDassboardByIdAsync(staffId);

            Assert.Equal(0, result.TotalDoneTask);
            Assert.Equal(0, result.TotalCreateOrder);
            Assert.Equal(0, result.AfferageResolveTime);
        }
    }
}
