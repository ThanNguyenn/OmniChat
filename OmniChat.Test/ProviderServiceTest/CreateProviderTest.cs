using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Requests.Provider;
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

namespace OmniChat.Test.ProviderServiceTest
{
    public class CreateProviderTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<Provider>> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ProviderService _service;

        public CreateProviderTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<Provider>>();
            _mockMapper = new Mock<IMapper>();

            _mockUow.Setup(u => u.GetRepository<Provider>()).Returns(_mockRepo.Object);

            _mockUow.Setup(u => u.ProcessInTransactionAsync(It.IsAny<Func<Task<bool>>>()))
                .Returns<Func<Task<bool>>>(delegate (Func<Task<bool>> action)
                {
                    return action();
                });

            _service = new ProviderService(
                _mockUow.Object,
                new Mock<ILogger<ProviderService>>().Object,
                _mockMapper.Object,
                new Mock<IHttpContextAccessor>().Object);
        }

        [Fact]
        public async Task CreateProviderAsync_ValidRequest_ReturnsTrue()
        {

            var request = new CreateProviderRequest { ProviderName = "Zalo" };
            var newProvider = new Provider { Id = Guid.NewGuid(), ProviderName = "Zalo" };


            _mockRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Provider, bool>>>(),
                null,
                null
            )).ReturnsAsync((Provider)null);

            _mockMapper.Setup(m => m.Map<Provider>(request)).Returns(newProvider);


            var result = await _service.CreateProviderAsync(request);


            Assert.True(result);
            _mockRepo.Verify(r => r.InsertAsync(It.IsAny<Provider>()), Times.Once);
        }

        [Fact]
        public async Task CreateProviderAsync_ProviderNameEmpty_ThrowsBadRequestException()
        {

            var request = new CreateProviderRequest { ProviderName = "" };


            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateProviderAsync(request));

            Assert.Equal("ứng dụng liên kết không được để trống.", exception.Message);
        }

        [Fact]
        public async Task CreateProviderAsync_NameAlreadyExist_ThrowsBadRequestException()
        {
           
            var request = new CreateProviderRequest { ProviderName = "Facebook" };
            var existingProvider = new Provider { ProviderName = "Facebook" };

            _mockRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<Provider, bool>>>(),
                null,
                null
            )).ReturnsAsync(existingProvider);


            var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
                _service.CreateProviderAsync(request));

            Assert.Equal("ứng dụng liên kết này đã tồn tại trong hệ thống.", exception.Message);
        }
    }
}
