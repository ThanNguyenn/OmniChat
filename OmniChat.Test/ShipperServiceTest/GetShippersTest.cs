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

namespace OmniChat.Test.ShipperServiceTest
{
    public class GetShippersTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<Staff>> _mockRepo;
        private readonly Mock<IR2StorageService> _mockStorage;
        private readonly StaffService _service;

        public GetShippersTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<Staff>>();
            _mockStorage = new Mock<IR2StorageService>();


            _mockUow.Setup(u => u.GetRepository<Staff>()).Returns(_mockRepo.Object);


            _service = new StaffService(
                _mockUow.Object,
                new Mock<ILogger<StaffService>>().Object,
                new Mock<IMapper>().Object,
                new Mock<IHttpContextAccessor>().Object,
                _mockStorage.Object 
            );
        }

        [Fact]
        public async Task GetShippersAsync_ValidData_ReturnsCorrectPagingAndStats()
        {
       
            int pageIndex = 1;
            int pageSize = 10;

            var role = new Role { Name = "Shipper" };

            var staffList = new List<Staff>
            {
                new Staff
                {
                    Id = Guid.NewGuid(),
                    Name = "Shipper Thành Công",
                    IsActive = true,
                    Account = new Account { Role = role },
                    OrdersAsDriver = new List<Order>
                    {
                       
                        new Order { Status = OrderStatus.Pending, DeliveryStatus = DeliveryStatus.Pending },
                     
                        new Order { DeliveryStatus = DeliveryStatus.Completed }
                    }
                },
                new Staff
                {
                    Id = Guid.NewGuid(),
                    Name = "Shipper Mới",
                    IsActive = true,
                    Account = new Account { Role = role },
                    OrdersAsDriver = new List<Order>()
                }
            };

           
            var mockQueryable = staffList.AsQueryable().BuildMock();
            _mockRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IQueryable<Staff>>>(),
                It.IsAny<bool>()
            )).Returns(mockQueryable);

            
            var result = await _service.GetShippersAsync(pageIndex, pageSize);

           
            Assert.NotNull(result);
            Assert.Equal(2, result.Meta.TotalItems);
            Assert.Equal(2, result.Items.Count());

            var shipperStats = result.Items.First(s => s.ShipperName == "Shipper Thành Công");
            Assert.Equal(1, shipperStats.TotalOrderShipNow);
            Assert.Equal(1, shipperStats.TotalOrderShipped);
        }

        [Fact]
        public async Task GetShippersAsync_NoActiveShipper_ReturnsEmptyItems()
        {
           
            var staffList = new List<Staff>
            {
               
                new Staff { IsActive = false, Account = new Account { Role = new Role { Name = "Shipper" } } },
               
                new Staff { IsActive = true, Account = new Account { Role = new Role { Name = "Admin" } } }
            };

            var mockQueryable = staffList.AsQueryable().BuildMock();
            _mockRepo.Setup(r => r.GetQueryable(
                 It.IsAny<Expression<Func<Staff, bool>>>(),
                 It.IsAny<Func<IQueryable<Staff>, IQueryable<Staff>>>(),
                 It.IsAny<bool>()
             )).Returns(mockQueryable);

            
            var result = await _service.GetShippersAsync(1, 10);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.Meta.TotalItems);
        }

        [Fact]
        public async Task GetShippersAsync_PaginationLogic_CalculatesCorrectTotalPages()
        {

            var role = new Role { Name = "Shipper" };
            var staffList = Enumerable.Range(1, 5).Select(i => new Staff
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                Account = new Account { Role = role },
                OrdersAsDriver = new List<Order>()
            }).ToList();

            var mockQueryable = staffList.AsQueryable().BuildMock();
            _mockRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IQueryable<Staff>>>(),
                It.IsAny<bool>()
            )).Returns(mockQueryable);

            var result = await _service.GetShippersAsync(pageIndex: 1, pageSize: 2);


            Assert.Equal(5, result.Meta.TotalItems);
            Assert.Equal(3, result.Meta.TotalPages); 
            Assert.Equal(2, result.Items.Count());
        }
    }
}
