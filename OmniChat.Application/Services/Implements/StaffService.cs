using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Utils;
using OmniChat.Infrastructure.Dtos.Requests.Staff;
using OmniChat.Infrastructure.Dtos.Responses.Staff;
using OmniChat.Infrastructure.Exceptions;
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

namespace OmniChat.Application.Services.Implements;

public class StaffService : BaseService<StaffService>, IStaffService
{
    public StaffService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<StaffService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task<bool> CreateStaffAsync(CreateStaffRequest createStaffRequest)
    {
        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var staffRepo = _unitOfWork.GetRepository<Staff>();
            var accountRepo = _unitOfWork.GetRepository<Account>();

            var existingStaff = await staffRepo.SingleOrDefaultAsync(predicate: s => s.Email == createStaffRequest.Email || s.Phone == createStaffRequest.Phone);
            if (existingStaff != null)
            {
                throw new BusinessException("Staff with the same email or phone number already exists.");
            }

            var staff = _mapper.Map<Staff>(createStaffRequest);

            var account = await CreateAccountForStaffAsync(createStaffRequest);
            await accountRepo.InsertAsync(account);
            await _unitOfWork.CommitAsync();

            staff.AccountId = account.Id;
            await staffRepo.InsertAsync(staff);

            return true;
        });
    }

    private async Task<Account> CreateAccountForStaffAsync(CreateStaffRequest request)
    {
        var defaultPassword = "Omnichat@0294"; // temp
        var hashedPassword = await PasswordUtil.HashPassword(defaultPassword);

        return new Account
        {
            UserName = request.Email,
            Password = hashedPassword,
            RoleId = request.RoleId
        };
    }

    public async Task<bool> UpdateStaffAsync(Guid StaffId, UpdateStaffRequest updateStaffRequest)
    {
        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var staffRepository = _unitOfWork.GetRepository<Staff>();
            var existingStaff = await staffRepository.GetByIdAsync(StaffId);
            if (existingStaff == null)
            {
                throw new BusinessException("Staff not found.");
            }
            _mapper.Map(updateStaffRequest, existingStaff);
            staffRepository.Update(existingStaff);
            return true;
        });
    }

    public async Task<bool> DeleteStaffAsync(Guid StaffId)
    {
        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var staffRepository = _unitOfWork.GetRepository<Staff>();
            var existingStaff = await staffRepository.GetByIdAsync(StaffId);
            if (existingStaff == null)
            {
                throw new BusinessException("Staff not found.");
            }
            staffRepository.Delete(existingStaff);
            return true;
        });
    }


    public async Task<PagingResponse<GetStaffsResponse>> GetStaffsAsync(Guid deparmentId, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false)
    {
        var staffRepository = _unitOfWork.GetRepository<Staff>();
        var departmentRepository = _unitOfWork.GetRepository<KeywordTypes>();
        var existingDepartment = await departmentRepository.GetByIdAsync(deparmentId) ?? throw new NotFoundException("Department id not exist");

        var response = await staffRepository.GetPagingListAsync<GetStaffsResponse>(
            predicate: s =>  s.IsActive == true,
            orderBy: q => OrderBy(q, sortBy, descending),
            selector: e => _mapper.Map<GetStaffsResponse>(e),
            page: pageNumber,
            size: pageSize
        );
        return response;
    }

    private static IOrderedQueryable<Staff> OrderBy(IQueryable<Staff> query,string sortBy,bool descending)
    {
        sortBy = sortBy?.Trim().ToLower() ?? "id";

        Expression<Func<Staff, object>> keySelector = sortBy switch
        {
            "name" => s => s.Name,
            "email" => s => s.Email,
            "phone" => s => s.Phone,
            "status" => s => s.Status,
            _ => s => s.Id
        };

        return descending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }

}
