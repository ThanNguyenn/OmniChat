using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
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

namespace OmniChat.Test.StaffPeformanceServiceTest
{
    public class InitializePerformanceStaffTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<Staff>> _mockStaffRepo;
        private readonly Mock<IGenericRepository<StaffPerformance>> _mockPerformanceRepo;
        private readonly Mock<ILogger<StaffPerformanceService>> _mockLogger; 
        private readonly StaffPerformanceService _service;

        public InitializePerformanceStaffTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockStaffRepo = new Mock<IGenericRepository<Staff>>();
            _mockPerformanceRepo = new Mock<IGenericRepository<StaffPerformance>>();
            _mockLogger = new Mock<ILogger<StaffPerformanceService>>();

            _mockUow.Setup(u => u.GetRepository<Staff>()).Returns(_mockStaffRepo.Object);
            _mockUow.Setup(u => u.GetRepository<StaffPerformance>()).Returns(_mockPerformanceRepo.Object);

            _service = new StaffPerformanceService(
                _mockUow.Object,
                _mockLogger.Object,
                new Mock<IMapper>().Object,
                new Mock<IHttpContextAccessor>().Object
            );
        }

        [Fact]
        public async Task InitializePerformanceForStaffAsync_StaffNotFound_ThrowsNotFoundException()
        {
            var staffId = Guid.NewGuid();
            _mockStaffRepo.Setup(r => r.GetByIdAsync(staffId)).ReturnsAsync((Staff)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.InitializePerformanceForStaffAsync(staffId));

            Assert.Equal("Nhân viên không tìm thấy hoặc đã bị xóa.", ex.Message);
        }

        [Fact]
        public async Task InitializePerformanceForStaffAsync_PerformanceNotExists_InsertsNewPerformance()
        {
            var staffId = Guid.NewGuid();
            var staff = new Staff { Id = staffId };

            _mockStaffRepo.Setup(r => r.GetByIdAsync(staffId)).ReturnsAsync(staff);

            _mockPerformanceRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<StaffPerformance, bool>>>(),
                null,
                null
            )).ReturnsAsync((StaffPerformance)null);

            // Act
            await _service.InitializePerformanceForStaffAsync(staffId);

            _mockPerformanceRepo.Verify(r => r.InsertAsync(It.Is<StaffPerformance>(p => p.StaffId == staffId)), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task InitializePerformanceForStaffAsync_PerformanceAlreadyExists_DoesNotInsert()
        {
            var staffId = Guid.NewGuid();
            var staff = new Staff { Id = staffId };
            var existingPerformance = new StaffPerformance { StaffId = staffId };

            _mockStaffRepo.Setup(r => r.GetByIdAsync(staffId)).ReturnsAsync(staff);

          
            _mockPerformanceRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<StaffPerformance, bool>>>(),
                null,
                null
            )).ReturnsAsync(existingPerformance);

            await _service.InitializePerformanceForStaffAsync(staffId);

           
            _mockPerformanceRepo.Verify(r => r.InsertAsync(It.IsAny<StaffPerformance>()), Times.Never);
            _mockUow.Verify(u => u.CommitAsync(), Times.Never);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Performance already exists")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
