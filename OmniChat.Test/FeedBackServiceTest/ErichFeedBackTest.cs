using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Dtos.Requests.FeedBack;
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
    public class ErichFeedBackTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<FeedBack>> _mockFeedbackRepo;
        private readonly Mock<IGenericRepository<SupportConversation>> _mockConversationRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly FeedBackService _service;

        public ErichFeedBackTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockFeedbackRepo = new Mock<IGenericRepository<FeedBack>>();
            _mockConversationRepo = new Mock<IGenericRepository<SupportConversation>>();
            _mockMapper = new Mock<IMapper>();

         
            _mockUow.Setup(u => u.GetRepository<FeedBack>()).Returns(_mockFeedbackRepo.Object);
            _mockUow.Setup(u => u.GetRepository<SupportConversation>()).Returns(_mockConversationRepo.Object);

            _service = new FeedBackService(
                _mockUow.Object,
                new Mock<ILogger<FeedBackService>>().Object,
                _mockMapper.Object,
                new Mock<IHttpContextAccessor>().Object);
        }

        [Fact]
        public async Task ErichFeedBackFormAsync_ValidRequest_ReturnsTrue()
        {
            
            var conversationId = Guid.NewGuid();
            var staffId = Guid.NewGuid();
            var formUrl = "https://test-form.vercel.app?id=" + conversationId;
            var request = new FeedBackRequest
            {
                Content = "Dịch vụ rất tốt",
                Rating = 5,
                CustomerEmail = "customer@gmail.com"
            };

            var conversation = new SupportConversation
            {
                Id = conversationId,
                ActiveStaffId = staffId
            };

            var feedbackEntity = new FeedBack();

            _mockConversationRepo.Setup(r => r.GetByIdAsync(conversationId)).ReturnsAsync(conversation);
            _mockMapper.Setup(m => m.Map<FeedBack>(request)).Returns(feedbackEntity);

            
            var result = await _service.ErichFeedBackFormAsync(conversationId, request, formUrl);

           
            Assert.True(result);
            Assert.Equal(staffId, feedbackEntity.StaffId);
            Assert.Equal(conversationId, feedbackEntity.SupportConversationId);
            Assert.Equal(formUrl, feedbackEntity.FormUrl);

            _mockFeedbackRepo.Verify(r => r.InsertAsync(feedbackEntity), Times.Once);
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task ErichFeedBackFormAsync_ConversationNotFound_ThrowsKeyNotFoundException()
        {
            
            var conversationId = Guid.NewGuid();
            _mockConversationRepo.Setup(r => r.GetByIdAsync(conversationId)).ReturnsAsync((SupportConversation)null);

            
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.ErichFeedBackFormAsync(conversationId, new FeedBackRequest(), "url"));
        }

        [Fact]
        public async Task ErichFeedBackFormAsync_NoActiveStaff_ThrowsInvalidOperationException()
        {
          
            var conversationId = Guid.NewGuid();
            var conversation = new SupportConversation
            {
                Id = conversationId,
                ActiveStaffId = null 
            };

            _mockConversationRepo.Setup(r => r.GetByIdAsync(conversationId)).ReturnsAsync(conversation);

           
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.ErichFeedBackFormAsync(conversationId, new FeedBackRequest(), "url"));

            Assert.Equal("Cuộc hội thoại này chưa có nhân viên phụ trách, không thể tạo phản hồi.", exception.Message);
        }
    }
}
