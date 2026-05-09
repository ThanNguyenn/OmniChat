using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Responses.CustomerProfile;
using OmniChat.Infrastructure.Dtos.Responses.Wallet;
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

namespace OmniChat.Test.CustomerProfileServiceTest
{
    public class GetCustomerProfilesPagingTest
    {
        private readonly Mock<IUnitOfWork<OmniChatDbContext>> _mockUow;
        private readonly Mock<IGenericRepository<CustomerProfile>> _mockRepo;
        private readonly Mock<IWalletService> _mockWalletService;
        private readonly Mock<IHubContext<SupportConversationHub>> _mockHubContext;
        private readonly Mock<IHttpContextAccessor> _mockAccessor;
        private readonly CustomerProfileService _service;

        public GetCustomerProfilesPagingTest()
        {
            _mockUow = new Mock<IUnitOfWork<OmniChatDbContext>>();
            _mockRepo = new Mock<IGenericRepository<CustomerProfile>>();
            _mockWalletService = new Mock<IWalletService>();
            _mockHubContext = new Mock<IHubContext<SupportConversationHub>>();
            _mockAccessor = new Mock<IHttpContextAccessor>();

            _mockUow.Setup(u => u.GetRepository<CustomerProfile>()).Returns(_mockRepo.Object);

            _service = new CustomerProfileService(
                _mockUow.Object,
                new Mock<ILogger<CustomerProfileService>>().Object,
                new Mock<AutoMapper.IMapper>().Object,
                _mockAccessor.Object,
                _mockHubContext.Object,    
                _mockWalletService.Object  
            );
        }

        [Fact]
        public async Task GetCustomerProfilesPagingAsync_ValidSearch_ReturnsDataWithWallet()
        {
            
            var customerId = Guid.NewGuid();
            var customerName = "Nguyen Van A";

            var pagingResponse = new PagingResponse<GetCustomerProfileResponse>
            {
                Items = new List<GetCustomerProfileResponse>
                {
                    new GetCustomerProfileResponse
                    {
                        Id = customerId,
                        CustomerName = customerName
                    }
                },
                Meta = new PaginationMeta { TotalItems = 1, CurrentPage = 1, PageSize = 20 }
            };


            _mockRepo.Setup(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<CustomerProfile, GetCustomerProfileResponse>>>(),
                It.IsAny<Expression<Func<CustomerProfile, bool>>>(),
                It.IsAny<Func<IQueryable<CustomerProfile>, IOrderedQueryable<CustomerProfile>>>(),
                It.IsAny<Func<IQueryable<CustomerProfile>, IIncludableQueryable<CustomerProfile, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>()
            )).ReturnsAsync(pagingResponse);

            var walletData = new GetWalletResponse { Amount = 1000000 };
            _mockWalletService.Setup(s => s.CalculateWallet(customerId))
                             .ReturnsAsync(walletData);

            var result = await _service.GetCustomerProfilesPagingAsync(1, 20, customerName);

 
            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal(walletData, result.Items.First().getWalletResponse); 

            _mockWalletService.Verify(s => s.CalculateWallet(customerId), Times.Once);
            _mockRepo.Verify(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<CustomerProfile, GetCustomerProfileResponse>>>(),
                It.IsNotNull<Expression<Func<CustomerProfile, bool>>>(),
                It.IsAny<Func<IQueryable<CustomerProfile>, IOrderedQueryable<CustomerProfile>>>(),
                It.IsAny<Func<IQueryable<CustomerProfile>, IIncludableQueryable<CustomerProfile, object>>>(),
                1,
                20), Times.Once);
        }

        [Fact]
        public async Task GetCustomerProfilesPagingAsync_EmptySearchTerm_PassesNullPredicate()
        {

            var pagingResponse = new PagingResponse<GetCustomerProfileResponse>
            {
                Items = new List<GetCustomerProfileResponse>(),
                Meta = new PaginationMeta { TotalItems = 0 }
            };

            _mockRepo.Setup(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<CustomerProfile, GetCustomerProfileResponse>>>(),
                null, 
                It.IsAny<Func<IQueryable<CustomerProfile>, IOrderedQueryable<CustomerProfile>>>(),
                It.IsAny<Func<IQueryable<CustomerProfile>, IIncludableQueryable<CustomerProfile, object>>>(),
                1,
                20
            )).ReturnsAsync(pagingResponse);

            // Act
            await _service.GetCustomerProfilesPagingAsync(1, 20, "");

            // Assert
            _mockRepo.Verify(r => r.GetPagingListAsync(
                It.IsAny<Expression<Func<CustomerProfile, GetCustomerProfileResponse>>>(),
                null,
                It.IsAny<Func<IQueryable<CustomerProfile>, IOrderedQueryable<CustomerProfile>>>(),
                It.IsAny<Func<IQueryable<CustomerProfile>, IIncludableQueryable<CustomerProfile, object>>>(),
                1, 20), Times.Once);
        }
    }
}
