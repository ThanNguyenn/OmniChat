using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
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
    public class AssignOrderShipperTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<Order>> _mockOrderRepo;
        private readonly Mock<IGenericRepository<Staff>> _mockStaffRepo;
        private readonly StaffService _service;

        public AssignOrderShipperTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockOrderRepo = new Mock<IGenericRepository<Order>>();
            _mockStaffRepo = new Mock<IGenericRepository<Staff>>();

            _mockUow.Setup(u => u.GetRepository<Order>()).Returns(_mockOrderRepo.Object);
            _mockUow.Setup(u => u.GetRepository<Staff>()).Returns(_mockStaffRepo.Object);

            _service = new StaffService(
                _mockUow.Object,
                new Mock<ILogger<StaffService>>().Object,
                new Mock<AutoMapper.IMapper>().Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IR2StorageService>().Object);
        }

        [Fact]
        public async Task AssignShipperOrderAsync_ValidData_UpdatesOrderSuccessfully()
        {
            var shipperId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var order = new Order { Id = orderId, IsDeleted = false, DriverId = null };
            var staff = new Staff
            {
                Id = shipperId,
                IsActive = true,
                Status = StaffStatus.Online,
                Account = new Account { Role = new Role { Name = "Shipper" } }
            };

            _mockOrderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);

            _mockStaffRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Staff, bool>>>(),
                It.IsAny<Func<IQueryable<Staff>, IOrderedQueryable<Staff>>>(),
                It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()
            )).ReturnsAsync(staff);

            await _service.AssignShipperOrderAsync(shipperId, orderId);

            Assert.Equal(shipperId, order.DriverId);
            Assert.Equal(DeliveryStatus.Pending, order.DeliveryStatus);
            _mockOrderRepo.Verify(r => r.Update(order), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task AssignShipperOrderAsync_OrderNotFound_ThrowsNotFoundException()
        {
            var orderId = Guid.NewGuid();
            _mockOrderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync((Order)null);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.AssignShipperOrderAsync(Guid.NewGuid(), orderId));
        }

        [Fact]
        public async Task AssignShipperOrderAsync_StaffNotShipper_ThrowsNotFoundException()
        {
            var shipperId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var order = new Order { Id = orderId, IsDeleted = false };
            var staff = new Staff
            {
                Id = shipperId,
                Account = new Account { Role = new Role { Name = "Admin" } } 
            };

            _mockOrderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
            _mockStaffRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Staff, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()
            )).ReturnsAsync(staff);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.AssignShipperOrderAsync(shipperId, orderId));
            Assert.Equal("Không tìm thấy nhân viên", ex.Message);
        }

        [Fact]
        public async Task AssignShipperOrderAsync_OrderAlreadyAssigned_ThrowsBusinessException()
        {
            var shipperId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var order = new Order { Id = orderId, DriverId = Guid.NewGuid() }; 
            var staff = new Staff
            {
                Id = shipperId,
                IsActive = true,
                Status = StaffStatus.Online,
                Account = new Account { Role = new Role { Name = "Shipper" } }
            };

            _mockOrderRepo.Setup(r => r.GetByIdAsync(orderId)).ReturnsAsync(order);
            _mockStaffRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Staff, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<Staff>, IIncludableQueryable<Staff, object>>>()
            )).ReturnsAsync(staff);

            await Assert.ThrowsAsync<BusinessException>(() =>
                _service.AssignShipperOrderAsync(shipperId, orderId));
        }
    }
}
