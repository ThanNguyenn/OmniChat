using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Responses.FeedBack;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.FeedBackServiceTest
{
    public class GetFeedBackByIdTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<FeedBack>> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly FeedBackService _service;

        public GetFeedBackByIdTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<FeedBack>>();
            _mockMapper = new Mock<IMapper>();

            _mockUow.Setup(u => u.GetRepository<FeedBack>()).Returns(_mockRepo.Object);

            _service = new FeedBackService(
                _mockUow.Object,
                new Mock<ILogger<FeedBackService>>().Object,
                _mockMapper.Object,
                new Mock<IHttpContextAccessor>().Object);
        }

        [Fact]
        public async Task GetFeedBackByIdAsync_ExistingId_ReturnsFeedBackResponse()
        {
            
            var feedbackId = Guid.NewGuid();
            var feedback = new FeedBack
            {
                Id = feedbackId,
                Content = "Dịch vụ tuyệt vời",
                Rating = 5
            };

            var expectedResponse = new FeedBackResponse
            {
                Rating = 5,
                Content = "Dịch vụ tuyệt vời"
            };

            _mockRepo.Setup(r => r.GetByIdAsync(feedbackId)).ReturnsAsync(feedback);
            _mockMapper.Setup(m => m.Map<FeedBackResponse>(feedback)).Returns(expectedResponse);

            
            var result = await _service.GetFeedBackByIdAsync(feedbackId);

           
            Assert.NotNull(result);
            Assert.Equal(expectedResponse.Content, result.Content);
            Assert.Equal(expectedResponse.Rating, result.Rating);
            _mockRepo.Verify(r => r.GetByIdAsync(feedbackId), Times.Once);
        }

        [Fact]
        public async Task GetFeedBackByIdAsync_NonExistingId_ThrowsNotFoundException()
        {
          
            var feedbackId = Guid.NewGuid();

           
            _mockRepo.Setup(r => r.GetByIdAsync(feedbackId)).ReturnsAsync((FeedBack)null);

          
            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetFeedBackByIdAsync(feedbackId));

            Assert.Equal($"Không tìm thấy phản hồi với mã định danh: {feedbackId}", exception.Message);
            _mockRepo.Verify(r => r.GetByIdAsync(feedbackId), Times.Once);
            _mockMapper.Verify(m => m.Map<FeedBackResponse>(It.IsAny<FeedBack>()), Times.Never);
        }
    }
}
