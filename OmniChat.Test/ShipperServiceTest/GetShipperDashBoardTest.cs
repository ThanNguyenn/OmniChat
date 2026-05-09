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
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.ShipperServiceTest
{
    public class GetShipperDashBoardTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<Staff>> _mockStaffRepo;
        private readonly Mock<IGenericRepository<Order>> _mockOrderRepo;
        private readonly StaffService _service;

        public GetShipperDashBoardTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockStaffRepo = new Mock<IGenericRepository<Staff>>();
            _mockOrderRepo = new Mock<IGenericRepository<Order>>();

           
            _mockUow.Setup(u => u.GetRepository<Staff>()).Returns(_mockStaffRepo.Object);
            _mockUow.Setup(u => u.GetRepository<Order>()).Returns(_mockOrderRepo.Object);

            _service = new StaffService(
                _mockUow.Object,
                new Mock<ILogger<StaffService>>().Object,
                new Mock<IMapper>().Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IR2StorageService>().Object);
        }

        [Fact]
        public async Task GetShipperDashboardAsync_ShouldReturnCorrectStats()
        {
         
            var today = DateTime.UtcNow.Date;

            var role = new Role { Name = "Shipper" };
            var staffData = new List<Staff>
            {
                new Staff { IsActive = true, Status = StaffStatus.Online, Account = new Account { Role = role } },
                new Staff { IsActive = true, Status = StaffStatus.Online, Account = new Account { Role = role } },
                new Staff { IsActive = false, Status = StaffStatus.Online, Account = new Account { Role = role } }, 
                new Staff { IsActive = true, Status = StaffStatus.Offline, Account = new Account { Role = role } }  
            };
            var mockStaffQuery = staffData.AsQueryable().BuildMock();
      
            _mockStaffRepo.Setup(r => r.GetQueryable(null, null, false)).Returns(mockStaffQuery);

            var orderData = new List<Order>
            {
                new Order { DeliveryStatus = DeliveryStatus.Pending, DriverId = Guid.NewGuid(), IsDeleted = false },
                new Order { DeliveryStatus = DeliveryStatus.Pending, DriverId = Guid.NewGuid(), IsDeleted = false },
                
                new Order { DeliveryStatus = DeliveryStatus.Completed, DeliveriedDate = today, IsDeleted = false },
                
                new Order { DeliveryStatus = DeliveryStatus.Completed, DeliveriedDate = today.AddDays(-1), IsDeleted = false },
                
                new Order { DeliveryStatus = DeliveryStatus.Pending, DriverId = Guid.NewGuid(), IsDeleted = true }
            };
            var mockOrderQuery = orderData.AsQueryable().BuildMock();
            _mockOrderRepo.Setup(r => r.GetQueryable(null, null, false)).Returns(mockOrderQuery);

            var result = await _service.GetShipperDashboardAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.ActiveShippers);    
            Assert.Equal(2, result.DeliveringOrders);  
            Assert.Equal(1, result.DeliveredToday);   
        }

        [Fact]
        public async Task GetShipperDashboardAsync_WhenNoData_ReturnsZeroCounts()
        {
            
            var emptyStaff = new List<Staff>().AsQueryable().BuildMock();
            var emptyOrder = new List<Order>().AsQueryable().BuildMock();

            _mockStaffRepo.Setup(r => r.GetQueryable(null, null, false)).Returns(emptyStaff);
            _mockOrderRepo.Setup(r => r.GetQueryable(null, null, false)).Returns(emptyOrder);

            var result = await _service.GetShipperDashboardAsync();

         
            Assert.Equal(0, result.ActiveShippers);
            Assert.Equal(0, result.DeliveringOrders);
            Assert.Equal(0, result.DeliveredToday);
        }
    }
}
