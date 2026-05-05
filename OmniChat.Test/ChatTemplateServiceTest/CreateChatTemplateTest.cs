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
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace OmniChat.Test.ChatTemplateServiceTest
{
    public class CreateChatTemplateTest
    {

        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ChatTemplateService>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly Mock<IGenericRepository<ChatTemplate>> _mockRepo;
        private readonly ChatTemplateService _service;

        public CreateChatTemplateTest()
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
        public async Task CreateChatTemplateAsync_ShouldThrowBusinessException_WhenCodeAlreadyExists()
        {
            
            var request = new ChatTemplateRequest { Code = "CHAT01", Content = "Xin chào" };

            
            _mockRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<ChatTemplate, bool>>>(), 
                It.IsAny<Func<IQueryable<ChatTemplate>, IOrderedQueryable<ChatTemplate>>>(), 
                It.IsAny<Func<IQueryable<ChatTemplate>, IIncludableQueryable<ChatTemplate, object>>>()
            ))
            .ReturnsAsync(new ChatTemplate { Code = "CHAT01" });

            
            Func<Task> act = async () => await _service.CreateChatTemplateAsync(request);

            
            await act.Should().ThrowAsync<BusinessException>()
                     .WithMessage($"Mã Mẫu Chat '{request.Code}' đã tồn tại");
        }

        [Fact]
        public async Task CreateChatTemplateAsync_ShouldReturnTrue_WhenRequestIsValid()
        {
           
            var request = new ChatTemplateRequest { Code = "NEW01", Content = "Nội dung mới" };
            var chatEntity = new ChatTemplate { Code = "NEW01", Content = "Nội dung mới" };

           
            _mockRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<ChatTemplate, bool>>>(),
                It.IsAny<Func<IQueryable<ChatTemplate>, IOrderedQueryable<ChatTemplate>>>(),
                It.IsAny<Func<IQueryable<ChatTemplate>, IIncludableQueryable<ChatTemplate, object>>>()
            ))
            .ReturnsAsync((ChatTemplate)null); 

            _mockMapper.Setup(m => m.Map<ChatTemplate>(request)).Returns(chatEntity);

            // Act
            var result = await _service.CreateChatTemplateAsync(request);

            // Assert
            result.Should().BeTrue();
            _mockRepo.Verify(r => r.InsertAsync(It.IsAny<ChatTemplate>()), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Theory]
        [InlineData("", "Nội dung", "Mã Mẫu Chat là bắt buộc")] // Trống mã
        [InlineData("CHAT01", "", "Nội dung Mẫu Chat là bắt buộc")] // Trống nội dung
        [InlineData("TOO_LONG_CODE_123", "Nội dung", "Mã Mẫu Chat không hợp lệ")] // Sai Regex
        public void ChatTemplateRequest_Validation_ShouldReturnErrors(string code, string content, string expectedError)
        {
            // Arrange
            var request = new ChatTemplateRequest
            {
                Code = code,
                Content = content
            };
            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(request, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Any(r => r.ErrorMessage.Contains(expectedError)).Should().BeTrue();
        }

        [Fact]
        public void ChatTemplateRequest_ContentTooLong_ShouldFail()
        {
            // Arrange
            var request = new ChatTemplateRequest
            {
                Code = "C01",
                Content = new string('A', 501) // 501 ký tự
            };
            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(request, context, results, true);

            // Assert
            isValid.Should().BeFalse();
            results.Should().Contain(r => r.ErrorMessage.Contains("Nội dung không quá 500 ký tự"));
        }
    }
}
