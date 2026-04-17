using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Responses.Role;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;

namespace OmniChat.Application.Services.Implements;

public class RoleService : BaseService<RoleService>, IRoleService
{
    public RoleService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<RoleService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task<IEnumerable<GetRoleResponse>> GetRolesAsync()
    {
        var roleRepo = _unitOfWork.GetRepository<Role>();
        var roles = await roleRepo.GetListAsync();
        return _mapper.Map<IEnumerable<GetRoleResponse>>(roles);
    }
}
