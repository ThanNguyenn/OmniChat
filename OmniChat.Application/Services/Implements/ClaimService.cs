using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Claim;
using OmniChat.Infrastructure.Dtos.Responses.Claim;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Implements
{
    public class ClaimService : BaseService<ClaimService>, IClaimService
    {
        private readonly IStaffPerformanceService _staffPerformanceService;

        private readonly ISupportConversationService _supportConversationService;

        private readonly ISupportTaskService _supportTaskService;
        public ClaimService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<ClaimService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IStaffPerformanceService staffPerformanceService, ISupportConversationService supportConversationService, ISupportTaskService supportTaskService) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _staffPerformanceService = staffPerformanceService;
            _supportConversationService = supportConversationService;
            _supportTaskService = supportTaskService;
        }

        public async Task<bool> CreateClaimAsync(CreateClaimRequest claimRequest)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
             {
                 // call repo 
                 var _repo = _unitOfWork.GetRepository<Claim>();

                 // map entity

                 var entity = _mapper.Map<Claim>(claimRequest);

                 // Insert Database

                 await _repo.InsertAsync(entity);

                 return true;
             });
        }


        public async Task<IEnumerable<ClaimDetailResponse>> GetAllClaim()
        {
            var _repo = _unitOfWork.GetRepository<Claim>();

            var claims = await _repo.GetListAsync();

            return _mapper.Map<IEnumerable<ClaimDetailResponse>>(claims);
        }

        public async Task<bool> UpdateClaimInforAsync(Guid claimId, UpdateClaimRequest claimRequest)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                if (claimRequest == null)
                    throw new BadRequestException("New Claim is invalid");

                var claim = await GetPendingClaimAsync(claimId);

                _mapper.Map(claimRequest, claim);

                _unitOfWork.GetRepository<Claim>().Update(claim);

                return true;
            });
        }

        private async Task<Claim> GetPendingClaimAsync(Guid claimId)
        {
            var _repo = _unitOfWork.GetRepository<Claim>();

            var claim = await _repo.SingleOrDefaultAsync(predicate: c => c.Id == claimId);

            if (claim == null)
                throw new NotFoundException("Claim not found");

            if (claim.Status != ClaimStatus.Pending)
                throw new BadRequestException("Only pending claim can be modified");

            return claim;
        }

        public async Task<ClaimDetailResponse> ApproveClaimAsync(Guid claimId)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var claim = await GetPendingClaimAsync(claimId);

                claim.Status = ClaimStatus.Approved;

                _unitOfWork.GetRepository<Claim>().Update(claim);

                return _mapper.Map<ClaimDetailResponse>(claim);
            });
        }


        public async Task<ClaimDetailResponse> RejectClaimAsync(Guid claimId)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var claim = await GetPendingClaimAsync(claimId);

                claim.Status = ClaimStatus.Rejected;

                _unitOfWork.GetRepository<Claim>().Update(claim);

                return _mapper.Map<ClaimDetailResponse>(claim);
            });
        }

        public async Task<IEnumerable<StaffClaimResponse>> GetClaimsByStaffIdAsync(Guid staffId)
        {
            var repo = _unitOfWork.GetRepository<Claim>();

            var claims = await repo.GetListAsync(
                predicate: x => x.StaffId == staffId,
                include: q => q.Include(x => x.ClaimType),
                orderBy: q => q.OrderByDescending(x => x.SubmitDate)
            );

            if (!claims.Any())
                throw new NotFoundException("No claims found for this staff");

            return _mapper.Map<IEnumerable<StaffClaimResponse>>(claims);
        }

        public async Task ReAssignStaffAsync(Guid newStaffAssignId, Guid conversationReAssignId)
        {
            var converRepo = _unitOfWork.GetRepository<SupportConversation>();

            var staffRepo = _unitOfWork.GetRepository<Staff>();

            var supportTaskRepo = _unitOfWork.GetRepository<SupportTask>();

            var conversation = await converRepo.SingleOrDefaultAsync(predicate: cs => cs.Id == conversationReAssignId,
                include: c => c.Include(x => x.SupportTasks));

            if (conversation == null)
                throw new NotFoundException($"Conversation {conversationReAssignId} not found");

            var oldStaffId = conversation.ActiveStaffId;

            if (oldStaffId == null)
                throw new BusinessException("Conversation has no assigned staff to reassign");

            var newStaff = await staffRepo.SingleOrDefaultAsync(predicate: s => s.Id == newStaffAssignId,
                include: s => s.Include(x => x.StaffIntentTypes)
               );

            if (newStaff == null)
                throw new NotFoundException($"Staff {newStaffAssignId} not found");

            
            var conversationIntentTypeIds = conversation.SupportTasks
                .Where(t => t.Status != SupportTaskStatus.Done
                         && t.Status != SupportTaskStatus.Cancelled
                         && t.Status != SupportTaskStatus.closed)
                .Select(t => t.IntentTypeId)
                .Distinct()
                .ToHashSet();

            var newStaffIntentTypeIds = newStaff.StaffIntentTypes
                .Select(si => si.IntentTypeId)
                .ToHashSet();

            bool hasMatchingIntent = conversationIntentTypeIds
                 .Any(id => newStaffIntentTypeIds.Contains(id));

            if (!hasMatchingIntent)
                throw new BusinessException(
                 "New staff does not have any matching IntentType with this conversation's tasks");

            conversation.ActiveStaffId = newStaffAssignId;
            conversation.UpdateDate = DateTime.UtcNow;

            _unitOfWork.GetRepository<SupportConversation>().Update(conversation);

            var activeTasks = conversation.SupportTasks
               .Where(t => t.Status != SupportTaskStatus.Done
                        && t.Status != SupportTaskStatus.Cancelled
                        && t.Status != SupportTaskStatus.closed)
               .ToList();

            foreach (var task in activeTasks)
            {
                task.CurrentAssignedStaffId = newStaffAssignId;
                _unitOfWork.GetRepository<SupportTask>().Update(task);
            }

            var performanceList = await _unitOfWork.GetRepository<StaffPerformance>()
             .GetListAsync(predicate: sp => sp.StaffId == oldStaffId);

            var oldStaffPerformance = performanceList
                .OrderByDescending(sp => sp.CreateDate)
                .FirstOrDefault();

            if (oldStaffPerformance != null)
            {
                oldStaffPerformance.ReassignmentCount += 1;
                oldStaffPerformance.UpdateDate = DateTime.UtcNow;
                _unitOfWork.GetRepository<StaffPerformance>().Update(oldStaffPerformance);
            }
            else
            {
                // Chưa có performance record → tạo mới
                var newPerformance = new StaffPerformance
                {
                    Id = Guid.NewGuid(),
                    StaffId = oldStaffId.Value,
                    ReassignmentCount = 1,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };
                await _unitOfWork.GetRepository<StaffPerformance>().InsertAsync(newPerformance);
            }

            await _unitOfWork.CommitAsync();

            _logger.LogInformation(
                "[REASSIGN] Conversation {ConvId} reassigned from Staff {OldStaff} to Staff {NewStaff}. Tasks updated: {Count}",
                conversationReAssignId, oldStaffId, newStaffAssignId, activeTasks.Count);
        }
    }
}
