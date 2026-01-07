using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Utils;
using OmniChat.Infrastructure.Dtos.Requests.Account;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements;

public class AccountService : BaseService<AccountService>, IAccountService
{
    public AccountService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<AccountService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }
    public async Task<bool> CreateAccountAsync(CreateAccountRequest request)
    {
        var accountRepo = _unitOfWork.GetRepository<Account>();
        var staffRepo = _unitOfWork.GetRepository<Staff>();

        var staff = await staffRepo.SingleOrDefaultAsync(predicate: s => s.Id == request.StaffId)
            ?? throw new NotFoundException("Staff not found");

        if (staff.AccountId != null)
            throw new BusinessException("Account for this staff already exists");

        //var defaultPassword = PasswordUtil.GenerateDefaultPassword();
        var defaultPassword = "Omnichat@0294"; //Temporary hardcode password
        var hashedPassword = await PasswordUtil.HashPassword(defaultPassword);

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var account = new Account
            {
                UserName = staff.Email,
                Password = hashedPassword,
                RoleId = request.RoleId
            };

            await accountRepo.InsertAsync(account);

            staff.AccountId = account.Id;
            staffRepo.Update(staff);
        });
        return true;
    }
}
