using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.ChatTemplate;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.ChatTemplateServiceTest
{
    public class GetChatTemplateByIdTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ChatTemplateService>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<IGenericRepository<ChatTemplate>> _mockRepo;
        private readonly ChatTemplateService _service;

        public GetChatTemplateByIdTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ChatTemplateService>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();
            _mockRepo = new Mock<IGenericRepository<ChatTemplate>>();

            _mockUow.Setup(u => u.GetRepository<ChatTemplate>()).Returns(_mockRepo.Object);

            _service = new ChatTemplateService(
                _mockUow.Object,
                _mockLogger.Object,
                _mockMapper.Object,
                _mockAccessor.Object
            );
        }

        [Fact]
        public async Task GetChatTemplateByIdAsync_ShouldThrowNotFoundException_WhenIdDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ChatTemplate)null);

            // Act
            Func<Task> act = async () => await _service.GetChatTemplateByIdAsync(id);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("Không tìm thấy Mẫu Chat");
        }

        [Fact]
        public async Task GetChatTemplateByIdAsync_ShouldReturnResponse_WhenIdExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existingTemplate = new ChatTemplate
            {
                Id = id,
                Code = "O03",
                Content = "Đơn của quý khách đang được quản lí bên em xem xét ạ"
            };

            var expectedResponse = new ChatTemplateResponse
            {
                Id = id,
                Code = "O03",
                Content = "Đơn của quý khách đang được quản lí bên em xem xét ạ"
            };

            _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingTemplate);

            // Mock hành động Map từ Entity sang Response DTO
            _mockMapper.Setup(m => m.Map<ChatTemplateResponse>(existingTemplate))
                       .Returns(expectedResponse);

            // Act
            var result = await _service.GetChatTemplateByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Code.Should().Be("O03");
            result.Content.Should().Contain("xem xét");

            // Verify mapper được gọi đúng kiểu
            _mockMapper.Verify(m => m.Map<ChatTemplateResponse>(existingTemplate), Times.Once);
        }
    }
}
