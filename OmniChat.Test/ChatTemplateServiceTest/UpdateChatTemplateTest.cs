using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Requests.ChatTemplate;
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

namespace OmniChat.Test.ChatTemplateServiceTest
{
    public class UpdateChatTemplateTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ChatTemplateService>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<IGenericRepository<ChatTemplate>> _mockRepo;
        private readonly ChatTemplateService _service;


        public UpdateChatTemplateTest()
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
        public async Task UpdateChatTemplateAsync_ShouldThrowNotFoundException_WhenIdDoesNotExist()
        {

            var id = Guid.NewGuid();
            var request = new ChatTemplateRequest { Code = "UPD01", Content = "Updated" };


            _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ChatTemplate)null);


            Func<Task> act = async () => await _service.UpdateChatTemplateAsync(id, request);


            await act.Should().ThrowAsync<NotFoundException>().WithMessage("Không tìm thấy Mẫu Chat");
            _mockUow.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateChatTemplateAsync_ShouldThrowBusinessException_WhenNewCodeAlreadyExists()
        {

            var id = Guid.NewGuid();
            var request = new ChatTemplateRequest { Code = "EXIST01", Content = "Content" };
            var existingInDb = new ChatTemplate { Id = id, Code = "OLD01" };
            var duplicateInDb = new ChatTemplate { Id = Guid.NewGuid(), Code = "EXIST01" };

            _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingInDb);


            _mockRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<ChatTemplate, bool>>>(),
                It.IsAny<Func<IQueryable<ChatTemplate>, IOrderedQueryable<ChatTemplate>>>(),
                It.IsAny<Func<IQueryable<ChatTemplate>, IIncludableQueryable<ChatTemplate, object>>>()
            )).ReturnsAsync(duplicateInDb);


            Func<Task> act = async () => await _service.UpdateChatTemplateAsync(id, request);


            await act.Should().ThrowAsync<BusinessException>()
                     .WithMessage($"Mã Mẫu Chat '{request.Code}' đã tồn tại");
        }

        [Fact]
        public async Task UpdateChatTemplateAsync_ShouldReturnTrue_WhenUpdateIsSuccessful()
        {

            var id = Guid.NewGuid();
            var request = new ChatTemplateRequest { Code = "NEW01", Content = "New Content" };
            var existingTemplate = new ChatTemplate { Id = id, Code = "OLD01" };

            _mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingTemplate);


            _mockRepo.Setup(r => r.SingleOrDefaultAsync(
                 It.IsAny<Expression<Func<ChatTemplate, bool>>>(),
                 It.IsAny<Func<IQueryable<ChatTemplate>, IOrderedQueryable<ChatTemplate>>>(),
                 It.IsAny<Func<IQueryable<ChatTemplate>, IIncludableQueryable<ChatTemplate, object>>>()
             )).ReturnsAsync((ChatTemplate)null);


            var result = await _service.UpdateChatTemplateAsync(id, request);


            result.Should().BeTrue();
            _mockMapper.Verify(m => m.Map(request, existingTemplate), Times.Once);
            _mockRepo.Verify(r => r.Update(existingTemplate), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Theory]
        [InlineData("", "Content", "Mã Mẫu Chat là bắt buộc")]
        [InlineData("ABC", "", "Nội dung Mẫu Chat là bắt buộc")]
        [InlineData("WrongFormat123", "Content", "Mã Mẫu Chat không hợp lệ")]
        public void UpdateRequest_ShouldReturnValidationErrors(string code, string content, string expectedMessage)
        {
            var request = new ChatTemplateRequest { Code = code, Content = content };
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

            isValid.Should().BeFalse();
            results.Should().Contain(r => r.ErrorMessage.Contains(expectedMessage));
        }

        [Fact]
        public void ChatTemplateRequest_ContentTooLong_ShouldFail()
        {
            var longContent = new string('A', 501);
            var request = new ChatTemplateRequest { Code = "A1", Content = longContent };
            var context = new System.ComponentModel.DataAnnotations.ValidationContext(request);
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(request, context, results, true);

            isValid.Should().BeFalse();
            results.Should().Contain(r => r.ErrorMessage.Contains("Nội dung không quá 500 ký tự"));
        }
    }
}