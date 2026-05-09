using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.NotificationServiceTest
{
    public class GetNotificationByStaffIdTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<Notification>> _mockRepo;
        private readonly Mock<IMapper> _mockMapper; 
        private readonly NotificationService _service;

        public GetNotificationByStaffIdTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<Notification>>();
            _mockMapper = new Mock<IMapper>(); 

            
            _mockUow.Setup(u => u.GetRepository<Notification>()).Returns(_mockRepo.Object);

         
            _service = new NotificationService(
                _mockUow.Object,
                new Mock<ILogger<NotificationService>>().Object,
                _mockMapper.Object, 
                new Mock<IHttpContextAccessor>().Object);
        }

        [Fact]
        public async Task GetNotificationsByStaffIdAsync_ValidStaffId_ReturnsUnreadNotifications()
        {
            
            var staffId = Guid.NewGuid();
            var notifications = new List<Notification>
            {
                new Notification
                {
                    Id = Guid.NewGuid(),
                    StaffId = staffId,
                    MessageText = "Có tin nhắn mới",
                    IsRead = false,
                    CreatedDate = DateTime.UtcNow,
                    SupportConversation = new SupportConversation
                    {
                        CustomerName = "Nguyễn Văn A",
                        AvatarUrl = "img.png",
                        Providers = new Provider { ProviderName = "Zalo" }
                    }
                }
            };


            _mockRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Notification, bool>>>(),                                 
                It.IsAny<Func<IQueryable<Notification>, IOrderedQueryable<Notification>>>(),     
                It.IsAny<Func<IQueryable<Notification>, IIncludableQueryable<Notification, object>>>() 
            )).ReturnsAsync(notifications);

            
            var result = await _service.GetNotificationsByStaffIdAsync(staffId);

            
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("Có tin nhắn mới", resultList[0].Message);
            Assert.Equal("Nguyễn Văn A", resultList[0].CustomerName);
            Assert.Equal("Zalo", resultList[0].ProviderName);

            _mockRepo.Verify(r => r.GetListAsync(It.IsAny<Expression<Func<Notification, bool>>>(), It.IsAny<Func<IQueryable<Notification>, IOrderedQueryable<Notification>>>(), It.IsAny<Func<IQueryable<Notification>, IIncludableQueryable<Notification, object>>>()), Times.Once);
        }

        [Fact]
        public async Task GetNotificationsByStaffIdAsync_NoUnreadNotifications_ReturnsEmptyList()
        {
          
            var staffId = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Notification, bool>>>(),
                It.IsAny<Func<IQueryable<Notification>, IOrderedQueryable<Notification>>>(),
                It.IsAny<Func<IQueryable<Notification>, IIncludableQueryable<Notification, object>>>()
            )).ReturnsAsync(new List<Notification>());

          
            var result = await _service.GetNotificationsByStaffIdAsync(staffId);

          
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetNotificationsByStaffIdAsync_NullNavigationProperties_ReturnsDefaultValues()
        {
           
            var staffId = Guid.NewGuid();
            var notifications = new List<Notification>
            {
                new Notification
                {
                    MessageText = "Thông báo hệ thống",
                    SupportConversation = null, 
                    CreatedDate = null
                }
            };

            _mockRepo.Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Notification, bool>>>(),
                It.IsAny<Func<IQueryable<Notification>, IOrderedQueryable<Notification>>>(),
                It.IsAny<Func<IQueryable<Notification>, IIncludableQueryable<Notification, object>>>()
            )).ReturnsAsync(notifications);

           
            var result = await _service.GetNotificationsByStaffIdAsync(staffId);

          
            var item = result.First();
            Assert.Equal("Khách ẩn danh", item.CustomerName); 
            Assert.Equal("Hệ thống", item.ProviderName);   
            Assert.Equal(0, item.TimeStamp);              
        }
    }
}
