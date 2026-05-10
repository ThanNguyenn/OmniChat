using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Exceptions;
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
    public class GetShipperByIdTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<Staff>> _mockRepo;
        private readonly StaffService _service;

        public GetShipperByIdTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<Staff>>();

            _mockUow.Setup(u => u.GetRepository<Staff>()).Returns(_mockRepo.Object);

            _service = new StaffService(
                _mockUow.Object,
                new Mock<ILogger<StaffService>>().Object,
                new Mock<IMapper>().Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IR2StorageService>().Object);
        }

        [Fact]
        public async Task GetShipperByShipperIdAsync_ValidId_ReturnsShipperResponse()
        {
            var shipperId = Guid.NewGuid();
            var role = new Role { Name = "Shipper" };
            var account = new Account { Role = role };

            var staffData = new List<Staff>
            {
                new Staff
                {
                    Id = shipperId,
                    Name = "Shipper A",
                    IsActive = true,
                    Account = account,
                    Status = StaffStatus.Online,
                    OrdersAsDriver = new List<Order>
                    {
                        new Order { Status = OrderStatus.Pending, DeliveryStatus = DeliveryStatus.Pending },
                        new Order { DeliveryStatus = DeliveryStatus.Completed }
                    }
                }
            };

            var mockQueryable = staffData.AsQueryable().BuildMock();
            _mockRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IQueryable<Staff>>>(),
                It.IsAny<bool>()
            )).Returns(mockQueryable);

            var result = await _service.GetShipperByShipperIdAsync(shipperId);

            Assert.NotNull(result);
            Assert.Equal(shipperId, result.Id);
            Assert.Equal("Shipper A", result.ShipperName);
            Assert.Equal(1, result.TotalOrderShipNow);
            Assert.Equal(1, result.TotalOrderShipped);
        }

        [Fact]
        public async Task GetShipperByShipperIdAsync_InvalidIdOrNotShipper_ThrowsNotFoundException()
        {
            var shipperId = Guid.NewGuid();

            var staffData = new List<Staff>().AsQueryable().BuildMock();

            _mockRepo.Setup(r => r.GetQueryable(
                null, null, false
            )).Returns(staffData);

            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetShipperByShipperIdAsync(shipperId));

            Assert.Equal("Không tìm thấy nhân viên", exception.Message);
        }

        [Fact]
        public async Task GetShipperByShipperIdAsync_RoleNotMatch_ThrowsNotFoundException()
        {
            var staffId = Guid.NewGuid();
            var staffData = new List<Staff>
            {
                new Staff
                {
                    Id = staffId,
                    IsActive = true,
                    Account = new Account { Role = new Role { Name = "Admin" } }
                }
            }.AsQueryable().BuildMock();

            _mockRepo.Setup(r => r.GetQueryable(
                It.IsAny<Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IQueryable<Staff>>>(),
                It.IsAny<bool>()
            )).Returns(staffData);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetShipperByShipperIdAsync(staffId));
        }
    }
}
