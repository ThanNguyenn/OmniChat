using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.ConversationWarningServiceTest
{
    public class GetWarningByIdTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<ConversationWarning>> _mockWarningRepo;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly ConversationWarningService _service;

        public GetWarningByIdTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockWarningRepo = new Mock<IGenericRepository<ConversationWarning>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();

            _mockUow.Setup(u => u.GetRepository<ConversationWarning>()).Returns(_mockWarningRepo.Object);

            _service = new ConversationWarningService(
                _mockUow.Object,
                new Mock<ILogger<ConversationWarningService>>().Object,
                new Mock<AutoMapper.IMapper>().Object,
                _mockAccessor.Object);
        }

        [Fact]
        public async Task GetWarningByIdAsync_ExistingUnreviewedWarning_ReturnsResponseAndUpdatesStatus()
        {

            var id = Guid.NewGuid();
            var warning = new ConversationWarning
            {
                Id = id,
                IsReviewed = false, 
                WarningType = WarningType.StaffNotResponding,
                CreatedAt = DateTime.UtcNow,
                Staff = new Staff { Name = "Staff A" },
                Conversation = new SupportConversation
                {
                    CustomerProfile = new CustomerProfile { CustomerName = "Customer B" }
                }
            };

            _mockWarningRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<ConversationWarning, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<ConversationWarning>, IIncludableQueryable<ConversationWarning, object>>>()
            )).ReturnsAsync(warning);

            var result = await _service.GetWarningByIdAsync(id);


            Assert.NotNull(result);
            Assert.Equal("Customer B", result.CustomerName);
            Assert.True(warning.IsReviewed); 

            _mockWarningRepo.Verify(r => r.Update(warning), Times.Once); 
            _mockUow.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task GetWarningByIdAsync_AlreadyReviewedWarning_ReturnsResponseWithoutUpdate()
        {

            var id = Guid.NewGuid();
            var warning = new ConversationWarning
            {
                Id = id,
                IsReviewed = true,
                Staff = new Staff { Name = "Staff A" },
                Conversation = new SupportConversation { CustomerProfile = new CustomerProfile() }
            };

            _mockWarningRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<ConversationWarning, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<ConversationWarning>, IIncludableQueryable<ConversationWarning, object>>>()
            )).ReturnsAsync(warning);


            await _service.GetWarningByIdAsync(id);


            _mockWarningRepo.Verify(r => r.Update(It.IsAny<ConversationWarning>()), Times.Never); 
            _mockUow.Verify(u => u.CommitAsync(), Times.Never); 
        }

        [Fact]
        public async Task GetWarningByIdAsync_NotFound_ThrowsNotFoundException()
        {

            var id = Guid.NewGuid();
            _mockWarningRepo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<ConversationWarning, bool>>>(),
                null,
                It.IsAny<Func<IQueryable<ConversationWarning>, IIncludableQueryable<ConversationWarning, object>>>()
            )).ReturnsAsync((ConversationWarning)null);


            var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
                _service.GetWarningByIdAsync(id));

            Assert.Contains(id.ToString(), exception.Message);
        }
    }
}
