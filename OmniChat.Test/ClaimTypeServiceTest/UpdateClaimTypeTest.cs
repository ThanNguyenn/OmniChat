using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Requests.ClaimType;
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
    public class UpdateClaimTypeTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<IGenericRepository<ClaimType>> _mockClaimTypeRepo;
        private readonly Mock<ILogger<ClaimTypeService>> _mockLogger;

        private readonly ClaimTypeService _service;

        public UpdateClaimTypeTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockMapper = new Mock<IMapper>();
            _mockAccessor = new Mock<IHttpContextAccessor>();
            _mockClaimTypeRepo = new Mock<IGenericRepository<ClaimType>>();
            _mockLogger = new Mock<ILogger<ClaimTypeService>>();

            _mockUow.Setup(u => u.GetRepository<ClaimType>()).Returns(_mockClaimTypeRepo.Object);

           
            _mockUow.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
                    .Returns((Func<Task<bool>> func) => func());

            _service = new ClaimTypeService(
                _mockUow.Object,
                _mockLogger.Object,
                _mockMapper.Object,
                _mockAccessor.Object);
        }

        [Fact]
        public async Task UpdateClaimTypeAsync_ValidRequest_ReturnsTrue()
        {
          
            var id = Guid.NewGuid();
            var request = new ClaimTypeRequest { TypeName = "Updated Name" };
            var existingEntity = new ClaimType { Id = id, TypeName = "Old Name", IsActive = true };

            _mockClaimTypeRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingEntity);

          
            _mockMapper.Setup(m => m.Map(request, existingEntity)).Returns(existingEntity);

           
            var result = await _service.UpdateClaimTypeAsync(id, request);

           
            Assert.True(result);
            _mockClaimTypeRepo.Verify(r => r.Update(existingEntity), Times.Once);
        }

        [Fact]
        public async Task UpdateClaimTypeAsync_IdNotFound_ThrowsNotFoundException()
        {
            
            var id = Guid.NewGuid();
            _mockClaimTypeRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ClaimType)null);

            
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdateClaimTypeAsync(id, new ClaimTypeRequest()));

            Assert.Equal("Không tìm thấy loại khiếu nại hoặc loại khiếu nại đã bị ngưng hoạt động.", exception.Message);
        }

        [Fact]
        public async Task UpdateClaimTypeAsync_TypeInactive_ThrowsNotFoundException()
        {
            
            var id = Guid.NewGuid();
            var inactiveType = new ClaimType { Id = id, IsActive = false };

            _mockClaimTypeRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(inactiveType);

           
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.UpdateClaimTypeAsync(id, new ClaimTypeRequest()));
        }

        [Fact]
        public async Task UpdateClaimTypeAsync_NullRequest_ThrowsBadRequestException()
        {
           
            var id = Guid.NewGuid();
            var existingEntity = new ClaimType { Id = id, IsActive = true };

            _mockClaimTypeRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingEntity);

           
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.UpdateClaimTypeAsync(id, null));
        }
    }
}
