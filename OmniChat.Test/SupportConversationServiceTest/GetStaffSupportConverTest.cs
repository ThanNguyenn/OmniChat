using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Test.SupportConversationServiceTest
{
    public class GetStaffSupportConverTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _unitOfWorkMock;
        private readonly Mock<IGenericRepository<SupportConversation>> _repoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SupportConversationService _service;

        public GetStaffSupportConverTest()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _repoMock = new Mock<IGenericRepository<SupportConversation>>();
            _mapperMock = new Mock<IMapper>();
            var taskAssignment = new Mock<ITaskAssignmentService>();
            _service = new SupportConversationService(
                _unitOfWorkMock.Object,
                null,
                _mapperMock.Object, 
                null, 
                new Mock<ICustomerProfileService>().Object,
                new Mock<IHubContext<SidebarHub>>().Object,
                new Mock<ISupportTaskService>().Object,
                new Mock<INotificationService>().Object,
                taskAssignment.Object
            );
        }

        [Fact]
        public async Task GetStaffConversationAsync_ValidStaffId_ReturnsPagingResponse()
        {
            var staffId = Guid.NewGuid();
            int pageNumber = 1;
            int pageSize = 10;

            var mockItems = new List<StaffConversationResponse>
    {
        new StaffConversationResponse { ConversationId = Guid.NewGuid(), customerName = "Customer A" },
        new StaffConversationResponse { ConversationId = Guid.NewGuid(), customerName = "Customer B" }
    };

            var pagingResponse = new PagingResponse<StaffConversationResponse>
            {
                Items = mockItems,
                Meta = new PaginationMeta
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalItems = 2,
                    TotalPages = 1
                }
            };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);

            _repoMock.Setup(r => r.GetPagingListAsync<StaffConversationResponse>(
                It.IsAny<Expression<Func<SupportConversation, StaffConversationResponse>>>(), 
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),                  
                It.IsAny<Func<IQueryable<SupportConversation>, IOrderedQueryable<SupportConversation>>>(), 
                It.IsAny<Func<IQueryable<SupportConversation>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<SupportConversation, object>>>(), 
                pageNumber,
                pageSize
            )).ReturnsAsync(pagingResponse);

            var result = await _service.GetStaffConversationAsync(staffId, pageNumber, pageSize);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.Meta.CurrentPage.Should().Be(pageNumber); 
            result.Items.First().customerName.Should().Be("Customer A");
        }

        [Fact]
        public async Task GetStaffConversationAsync_NoData_ReturnsEmptyPagingResponse()
        {
            var staffId = Guid.NewGuid();
            var emptyResponse = new PagingResponse<StaffConversationResponse>
            {
                Items = new List<StaffConversationResponse>(),
                Meta = new PaginationMeta { TotalItems = 0, CurrentPage = 1 }
            };

            _unitOfWorkMock.Setup(u => u.GetRepository<SupportConversation>()).Returns(_repoMock.Object);

            _repoMock.Setup(r => r.GetPagingListAsync<StaffConversationResponse>(
                It.IsAny<Expression<Func<SupportConversation, StaffConversationResponse>>>(),
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IOrderedQueryable<SupportConversation>>>(),
                null, 
                It.IsAny<int>(),
                It.IsAny<int>()
            )).ReturnsAsync(emptyResponse);

            var result = await _service.GetStaffConversationAsync(staffId, 1, 10);

            result.Items.Should().BeEmpty();
            result.Meta.TotalItems.Should().Be(0);
        }
    }
}
