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
        var departmentRepository = _unitOfWork.GetRepository<IntentType>();
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

    public async Task<bool> AssignIntentToStaffAsync(Guid staffId, IEnumerable<AssignStaffToIntentTypeRequest> requests)
    {
        var staffIntentTypeRepo = _unitOfWork.GetRepository<StaffIntentType>();
        var staffRepo = _unitOfWork.GetRepository<Staff>();
        var intentTypeRepo = _unitOfWork.GetRepository<IntentType>();

        var existingStaff = await staffRepo.SingleOrDefaultAsync(predicate: s => s.Id == staffId && s.IsActive != false)
            ?? throw new NotFoundException($"Staff {staffId} not found or inactive");

        var intentIds = requests.Select(r => r.IntentId).Distinct().ToList();

        var activeIntents = await intentTypeRepo.GetListAsync(predicate: it => intentIds.Contains(it.Id) && it.IsActive != false);

        if (activeIntents.Count != intentIds.Count)
        {
            throw new NotFoundException("One or more intent types not found or inactive.");
        }

        var existingAssignmentIds = (await staffIntentTypeRepo.GetListAsync(predicate: sit =>
            sit.StaffId == staffId && intentIds.Contains(sit.IntentTypeId)))
            .Select(sit => sit.IntentTypeId)
            .ToList();

        var newIntentIds = intentIds.Except(existingAssignmentIds).ToList();

        if (newIntentIds.Any())
        {
            var newAssignments = newIntentIds.Select(id => new StaffIntentType
            {
                StaffId = staffId,
                IntentTypeId = id
            }).ToList();

            await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                await staffIntentTypeRepo.InsertRangeAsync(newAssignments);
            });
        }

        return true;
    }

    public async Task<bool> UnassignIntentFromStaffAsync(Guid staffId, AssignStaffToIntentTypeRequest unassignStaffFromIntentTypeRequest)
    {
        var staffIntentTypeRepo = _unitOfWork.GetRepository<StaffIntentType>();
        var staffRepo = _unitOfWork.GetRepository<Staff>();
        var intentTypeRepo = _unitOfWork.GetRepository<IntentType>();   

        var existingStaff = staffRepo.SingleOrDefaultAsync(predicate: s => s.Id == staffId && s.IsActive != false)
            ?? throw new NotFoundException($"Staff {staffId} not found or inactive");

        await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var existingAssignment = await staffIntentTypeRepo.SingleOrDefaultAsync(predicate: sit =>
                sit.StaffId == staffId && sit.IntentTypeId == unassignStaffFromIntentTypeRequest.IntentId) ?? throw new NotFoundException("Staff havent been assigned with this intent");

            staffIntentTypeRepo.Delete(existingAssignment);
        });
        return true;

    }
}
