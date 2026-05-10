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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.ClaimTypeServiceTest
{
    public class CreateClaimTypeTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<IGenericRepository<ClaimType>> _mockClaimTypeRepo;
        private readonly Mock<ILogger<ClaimTypeService>> _mockLogger;

        private readonly ClaimTypeService _service;

        public CreateClaimTypeTest()
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
        public async Task CreateNewClaimTypeAsync_ValidRequest_ReturnsTrue()
        {

            var request = new ClaimTypeRequest { TypeName = "New Claim Type" };
            var entity = new ClaimType { TypeName = "New Claim Type", IsActive = true };


            _mockClaimTypeRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<ClaimType, bool>>>(),
                null,
                null
            )).ReturnsAsync((ClaimType)null);

   
            _mockMapper.Setup(m => m.Map<ClaimType>(request)).Returns(entity);


            _mockClaimTypeRepo.Setup(r => r.InsertAsync(entity)).Returns(Task.CompletedTask);


            var result = await _service.CreateNewClaimTypeAsync(request);


            Assert.True(result);
            _mockClaimTypeRepo.Verify(r => r.InsertAsync(It.IsAny<ClaimType>()), Times.Once);
        }

        [Fact]
        public async Task CreateNewClaimTypeAsync_DuplicateName_ThrowsBadRequestException()
        {

            var request = new ClaimTypeRequest { TypeName = "Existing Type" };
            var existingEntity = new ClaimType { TypeName = "Existing Type", IsActive = true };


            _mockClaimTypeRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<ClaimType, bool>>>(),
                null,
                null
            )).ReturnsAsync(existingEntity);


            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateNewClaimTypeAsync(request));

            Assert.Equal("Loại khiếu nại này đã tồn tại trong hệ thống.", exception.Message);

            _mockClaimTypeRepo.Verify(r => r.InsertAsync(It.IsAny<ClaimType>()), Times.Never);
        }
    }
}
