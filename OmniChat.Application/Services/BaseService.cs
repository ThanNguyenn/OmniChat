using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services;

public abstract class BaseService<T> where T : class
{
    protected IUnitOfWork<OmniChatDbContext> _unitOfWork;
    protected ILogger<T> _logger;
    protected IMapper _mapper;
    protected IHttpContextAccessor _httpContextAccessor;

    public BaseService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<T> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }
}
