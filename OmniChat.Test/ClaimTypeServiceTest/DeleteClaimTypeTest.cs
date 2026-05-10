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
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.ClaimTypeServiceTest
{
    public class DeleteClaimTypeTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<IGenericRepository<ClaimType>> _mockClaimTypeRepo;
        private readonly Mock<ILogger<ClaimTypeService>> _mockLogger;

        private readonly ClaimTypeService _service;

        public DeleteClaimTypeTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();
            _mockClaimTypeRepo = new Mock<IGenericRepository<ClaimType>>();
            _mockLogger = new Mock<ILogger<ClaimTypeService>>();

           
            _mockUow.Setup(u => u.GetRepository<ClaimType>()).Returns(_mockClaimTypeRepo.Object);

           
            _mockUow.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
                    .Returns((Func<Task<bool>> func) => func());

            _service = new ClaimTypeService(
                _mockUow.Object,
                _mockLogger.Object,
                new Mock<AutoMapper.IMapper>().Object, 
                _mockAccessor.Object);
        }

        [Fact]
        public async Task DeleteClaimTypeByIdAsync_ValidId_ReturnsTrue()
        {

            var id = Guid.NewGuid();
            var existingEntity = new ClaimType { Id = id, IsActive = true, TypeName = "Test Type" };

            _mockClaimTypeRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingEntity);

            
            var result = await _service.DeleteClaimTypeByIdAsync(id);

            
            Assert.True(result);
            Assert.False(existingEntity.IsActive); 
            _mockClaimTypeRepo.Verify(r => r.Update(existingEntity), Times.Once);
        }

        [Fact]
        public async Task DeleteClaimTypeByIdAsync_IdNotFound_ThrowsNotFoundException()
        {
           
            var id = Guid.NewGuid();
            _mockClaimTypeRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ClaimType)null);

            
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DeleteClaimTypeByIdAsync(id));

            Assert.Equal("Loại khiếu nại không tồn tại hoặc đã được xóa trước đó.", exception.Message);
            _mockClaimTypeRepo.Verify(r => r.Update(It.IsAny<ClaimType>()), Times.Never);
        }

        [Fact]
        public async Task DeleteClaimTypeByIdAsync_AlreadyInactive_ThrowsNotFoundException()
        {
          
            var id = Guid.NewGuid();
            var inactiveType = new ClaimType { Id = id, IsActive = false };

            _mockClaimTypeRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(inactiveType);

           
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.DeleteClaimTypeByIdAsync(id));

            Assert.Equal("Loại khiếu nại không tồn tại hoặc đã được xóa trước đó.", exception.Message);
        }
    }
}
