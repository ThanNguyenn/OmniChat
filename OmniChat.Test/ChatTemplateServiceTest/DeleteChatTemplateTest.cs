using AutoMapper;
using FluentAssertions;
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

namespace OmniChat.Test.ChatTemplateServiceTest
{
    public class DeleteChatTemplateTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ChatTemplateService>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<IGenericRepository<ChatTemplate>> _mockRepo;
        private readonly ChatTemplateService _service;

        public DeleteChatTemplateTest()
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
        public async Task DeleteChatTemplateAsync_ShouldThrowNotFoundException_WhenIdDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Giả lập Repository không tìm thấy bản ghi
            _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ChatTemplate)null);

            // Act
            Func<Task> act = async () => await _service.DeleteChatTemplateAsync(id);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage("Không tìm thấy Mẫu Chat");

            // Verify: Đảm bảo không có lệnh Delete hay Commit nào được thực hiện
            _mockRepo.Verify(r => r.Delete(It.IsAny<ChatTemplate>()), Times.Never);
            _mockUow.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteChatTemplateAsync_ShouldReturnTrue_WhenDeleteIsSuccessful()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existingTemplate = new ChatTemplate { Id = id, Code = "DEL01", Content = "Xóa tôi đi" };

            // Giả lập tìm thấy bản ghi
            _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingTemplate);

            // Act
            var result = await _service.DeleteChatTemplateAsync(id);

            // Assert
            result.Should().BeTrue();

            // Verify: Kiểm tra xem repo.Delete và uow.CommitAsync có được gọi đúng 1 lần không
            _mockRepo.Verify(r => r.Delete(existingTemplate), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}
