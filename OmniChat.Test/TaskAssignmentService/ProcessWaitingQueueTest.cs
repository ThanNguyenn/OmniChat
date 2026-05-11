using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OmniChat.Application.Services.Implements;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System.Linq.Expressions;

namespace OmniChat.Test.TaskAssignmentServiceTest;

public class ProcessWaitingQueueTest
{
    private readonly Mock<IUnitOfWork<OmniChatDbContext>> _uowMock = new();
    private readonly Mock<ILogger<TaskAssignmentService>> _loggerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IHttpContextAccessor> _httpMock = new();
    private readonly Mock<HttpClient> _httpClientMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<IKeywordService> _keywordServiceMock = new();

    private Mock<IGenericRepository<SupportConversation>> SetupConversationRepo()
    {
        var repo = new Mock<IGenericRepository<SupportConversation>>();

        _uowMock.Setup(x => x.GetRepository<SupportConversation>())
            .Returns(repo.Object);

        return repo;
    }

    private TaskAssignmentService CreateService()
    {
        return new TaskAssignmentService(
            _uowMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _httpMock.Object,
            _httpClientMock.Object,
            _configMock.Object,
            _keywordServiceMock.Object
        );
    }

    [Fact]
    public async Task ProcessWaitingQueue_ShouldExit_WhenNoPendingConversation()
    {
        var repo = SetupConversationRepo();

        repo.Setup(r => r.SingleOrDefaultAsync(
                It.IsAny<Expression<Func<SupportConversation, bool>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IOrderedQueryable<SupportConversation>>>(),
                It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()))
            .ReturnsAsync((SupportConversation)null);

        var service = CreateService();

        await service.ProcessWaitingQueueAsync();

        repo.Verify(r => r.SingleOrDefaultAsync(
            It.IsAny<Expression<Func<SupportConversation, bool>>>(),
            It.IsAny<Func<IQueryable<SupportConversation>, IOrderedQueryable<SupportConversation>>>(),
            It.IsAny<Func<IQueryable<SupportConversation>, IIncludableQueryable<SupportConversation, object>>>()),
            Times.Once);
    }
}