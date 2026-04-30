using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.SignalRHub;
using OmniChat.Infrastructure.Dtos.Requests.Claim;
using OmniChat.Infrastructure.Dtos.Requests.TaskAction;
using OmniChat.Infrastructure.Dtos.Responses.Claim;
using OmniChat.Infrastructure.Dtos.Responses.Notification;
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
        private readonly ITaskActionService _taskActionService;
        private readonly IHubContext<SupportConversationHub> _hubContext;

        public ClaimService(IUnitOfWork<OmniChatDbContext> unitOfWork,
            ILogger<ClaimService> logger, 
            IMapper mapper, 
            IHttpContextAccessor httpContextAccessor,
            ITaskActionService taskActionService,
            IHubContext<SupportConversationHub> hubContext
            ) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _taskActionService = taskActionService;
            _hubContext = hubContext;
        }

        public async Task<bool> CreateClaimAsync(CreateClaimRequest claimRequest)
        {
            return await _unitOfWork.ProcessInTransactionAsync(async () =>
             {
                 var _taskRepo = _unitOfWork.GetRepository<SupportTask>();
                 var _conversationRepo = _unitOfWork.GetRepository<SupportConversation>();
                 var _repo = _unitOfWork.GetRepository<Claim>();
                 var _claimTypeRepo = _unitOfWork.GetRepository<ClaimType>();

               
                 var claimType = await _claimTypeRepo.GetByIdAsync(claimRequest.ClaimTypeId);
                 if (claimType == null)
                     throw new NotFoundException("Loại khiếu nại (Claim Type) không tồn tại.");

                 var changeTaskTypeId = Guid.Parse("abf8b2a1-0699-4c27-b241-11df7a75c12c");

                 if (claimType.Id == changeTaskTypeId)
                 {
                     if (!claimRequest.SupportConversationId.HasValue || claimRequest.SupportConversationId == Guid.Empty)
                     {
                        
                         throw new BadRequestException("Yêu cầu thay đổi công việc (CHANGETASK) bắt buộc phải đính kèm cuộc hội thoại hỗ trợ.");
                     }
                     var conversation = await _conversationRepo.SingleOrDefaultAsync(
                         predicate: x => x.Id == claimRequest.SupportConversationId.Value,
                            include: c => c.Include(conv => conv.SupportTasks)
                     );

                     if (conversation == null)
                         throw new NotFoundException("Không tìm thấy cuộc hội thoại được yêu cầu thay đổi.");

                     conversation.Status = ConversationStatus.PendingReassign;

                     foreach (var task in conversation.SupportTasks)
                     {
                         if (task.Status != SupportTaskStatus.Done && 
                         task.Status != SupportTaskStatus.Cancelled &&
                         task.Status != SupportTaskStatus.closed)
                         {
                             task.Status = SupportTaskStatus.PendingReassign ;
                         }
                     }
                     _taskRepo.UpdateRange(conversation.SupportTasks);
                      _conversationRepo.Update(conversation);

                 }
                 var entity = _mapper.Map<Claim>(claimRequest);
                 await _repo.InsertAsync(entity);         
                 return true;
             });
        }


        public async Task<PagingResponse<ClaimDetailResponse>> GetPendingChangeTask(int pageIndex = 1, int pageSize = 10)
        {
            var changeTaskTypeId = Guid.Parse("abf8b2a1-0699-4c27-b241-11df7a75c12c");
            var repo = _unitOfWork.GetRepository<Claim>();

          
            var query = repo.GetQueryable()
                .AsNoTracking()
                .Include(c => c.Staff)
                .Include(c => c.ClaimType)
                .Include(c => c.SupportConversation)
                .Where(c => c.Status == ClaimStatus.Pending && c.ClaimTypeId == changeTaskTypeId);

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.SubmitDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ClaimDetailResponse>>(items);

            return new PagingResponse<ClaimDetailResponse>
            {
                Items = mapped,
                Meta = new PaginationMeta
                {
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
                }
            };
        }

        public async Task<ClaimDashboardResponse> GetClaimDashboardAsync()
        {
            var repo = _unitOfWork.GetRepository<Claim>();

            var query = repo.GetQueryable();

            var pending = await query.CountAsync(c => c.Status == ClaimStatus.Pending);

            var approved = await query.CountAsync(c => c.Status == ClaimStatus.Approved);

            var rejected = await query.CountAsync(c => c.Status == ClaimStatus.Rejected);

            return new ClaimDashboardResponse
            {
                PendingClaims = pending,
                ApprovedClaims = approved,
                RejectedClaims = rejected
            };
        }

        public async Task<PagingResponse<ClaimDetailResponse>> GetPendingClaimAsync(int pageIndex = 1, int pageSize = 10)
        {
            var repo = _unitOfWork.GetRepository<Claim>();

            var query = repo.GetQueryable()
                .Include(c => c.Staff)      
                .Include(c => c.ClaimType)
                .Where(c => c.Status == ClaimStatus.Pending);

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.SubmitDate) 
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ClaimDetailResponse>>(items);

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PagingResponse<ClaimDetailResponse>
            {
                Items = mapped,
                Meta = new PaginationMeta
                {
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
        }


        public async Task<PagingResponse<ClaimDetailResponse>> GetClaimHistoryAsync(int pageIndex = 1, int pageSize = 10)
        {
            var repo = _unitOfWork.GetRepository<Claim>();

            var query = repo.GetQueryable()
                .Include(c => c.Staff)
                .Include(c => c.ClaimType)
                .Where(c => c.Status != ClaimStatus.Pending);

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.SubmitDate) 
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ClaimDetailResponse>>(items);

            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PagingResponse<ClaimDetailResponse>
            {
                Items = mapped,
                Meta = new PaginationMeta
                {
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
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

            var claim = await _repo.SingleOrDefaultAsync(predicate: c => c.Id == claimId,
                include: c => c.Include(c => c.Staff)
                               .Include(c => c.ClaimType) 
                
                );

            if (claim == null)
                throw new NotFoundException("Không tìm thấy yêu cầu khiếu nại");

            if (claim.Status != ClaimStatus.Pending)
                throw new BadRequestException("Chỉ có yêu cầu đang chờ xử lý mới có thể được sửa đổi");

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

        public async Task<PagingResponse<ClaimDetailResponse>> GetClaimsByStaffIdAsync(Guid staffId, int pageIndex = 1, int pageSize = 10)
        {
            var repo = _unitOfWork.GetRepository<Claim>();
            var query = repo.GetQueryable()
                .Include(c => c.Staff)
                .Include(c => c.ClaimType)
                .Where(c => c.StaffId == staffId);

            var totalItems = await query.CountAsync();

            if (totalItems == 0)
                throw new NotFoundException("Không tìm thấy yêu cầu khiếu nại cho nhân viên này");

            var items = await query
                .OrderByDescending(c => c.SubmitDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var mapped = _mapper.Map<IEnumerable<ClaimDetailResponse>>(items);
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return new PagingResponse<ClaimDetailResponse>
            {
                Items = mapped,
                Meta = new PaginationMeta
                {
                    CurrentPage = pageIndex,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                }
            };
        }

        public async Task ReAssignStaffAsync(Guid claimId,Guid newStaffAssignId, Guid conversationReAssignId)
        {
            var converRepo = _unitOfWork.GetRepository<SupportConversation>();

            var staffRepo = _unitOfWork.GetRepository<Staff>();

            var supportTaskRepo = _unitOfWork.GetRepository<SupportTask>();

            var claimRepo = _unitOfWork.GetRepository<Claim>();

            var claim = await claimRepo.SingleOrDefaultAsync(predicate: c => c.Id == claimId);
            if (claim == null) throw new NotFoundException($"Không tìm thấy yêu cầu khiếu nại (ID: {claimId})");

            claim.Status = ClaimStatus.Approved;
            claimRepo.Update(claim);

            var conversation = await converRepo.SingleOrDefaultAsync(predicate: cs => cs.Id == conversationReAssignId,
                include: c => c.Include(x => x.SupportTasks));
            if (conversation == null) throw new NotFoundException($"Hội thoại không tồn tại hoặc đã bị xóa.");

            var oldStaffId = conversation.ActiveStaffId;
            if (oldStaffId == null)
                throw new BadRequestException("Cuộc hội thoại này hiện không có nhân viên phụ trách để thực hiện bàn giao.");

            var newStaff = await staffRepo.SingleOrDefaultAsync(predicate: s => s.Id == newStaffAssignId,
         include: s => s.Include(x => x.StaffIntentTypes));
            if (newStaff == null) throw new NotFoundException($"Nhân viên mới nhận bàn giao không tồn tại trong hệ thống.");


           
            var newStaffCurrentWorkload = await converRepo.CountAsync(
            predicate: c => c.ActiveStaffId == newStaffAssignId &&
                    (c.Status == ConversationStatus.Pending &&
                                   c.Id != conversation.Id));

            var statusAfterApprove = newStaffCurrentWorkload >= 5
                   ? ConversationStatus.Warning
                   : ConversationStatus.Pending;

            conversation.ActiveStaffId = newStaffAssignId;
            conversation.Status = statusAfterApprove;
            conversation.UpdateDate = DateTime.UtcNow;
            conversation.LastStaffMessageAt = DateTime.UtcNow;
            converRepo.Update(conversation);

            var activeTasks = conversation.SupportTasks
               .Where(t => t.Status != SupportTaskStatus.Done
                        && t.Status != SupportTaskStatus.Cancelled
                        && t.Status != SupportTaskStatus.closed)
               .ToList();

            foreach (var task in activeTasks)
            {
                task.CurrentAssignedStaffId = newStaffAssignId;
                task.Status = SupportTaskStatus.Reassign;
                _unitOfWork.GetRepository<SupportTask>().Update(task);
                await _taskActionService.CreateTaskActionAsync(new TaskActionRequest
                {
                    SupportTaskId = task.Id,
                    Action = TaskActionType.Reassigned,
                    ActionById = oldStaffId.Value,
                    ActionToId = newStaffAssignId,
                    Reason = $"Chuyển giao từ nhân viên {oldStaffId} sang {newStaffAssignId} theo yêu cầu thay đổi công việc."
                });
            }

            var now = DateTime.UtcNow;
            var oldStaffPerformance = await _unitOfWork.GetRepository<StaffPerformance>()
                .SingleOrDefaultAsync(predicate: sp =>
                    sp.StaffId == oldStaffId &&
                    sp.FromTime <= now &&
                    sp.ToTime >= now);

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
                    StaffId = oldStaffId.Value,
                    ReassignmentCount = 1,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                };
                await _unitOfWork.GetRepository<StaffPerformance>().InsertAsync(newPerformance);
            }

            await _unitOfWork.CommitAsync();

            var notificationResponse = new ClaimNotificationResponse
            {
                ConversationName = conversation.CustomerName,
                Description = claim.Description ?? "Yêu cầu thay đổi công việc được phê duyệt.",
                Status = claim.Status,
                NewStatus = conversation.Status.ToString(),
                Message = "Bạn có một cuộc hội thoại mới được bàn giao."
            };

           
            await _hubContext.Clients
                .User(newStaffAssignId.ToString())
                .SendAsync("ReassignApproved", notificationResponse); 

            _logger.LogInformation(
                "[REASSIGN] Conversation {ConvId} reassigned from Staff {OldStaff} to Staff {NewStaff}. Tasks updated: {Count}",
                conversationReAssignId, oldStaffId, newStaffAssignId, activeTasks.Count);
        }


        public async Task RejectReassignClaimAsync(Guid claimId, Guid ManagerId)
        {
            await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                var claimRepo = _unitOfWork.GetRepository<Claim>();
                var conversationRepo = _unitOfWork.GetRepository<SupportConversation>();
                var taskRepo = _unitOfWork.GetRepository<SupportTask>();

         
                var claim = await claimRepo.GetByIdAsync(claimId);
                if (claim == null) throw new NotFoundException("Không tìm thấy yêu cầu chuyển giao công việc.");

                claim.Status = ClaimStatus.Rejected;
                claimRepo.Update(claim);

                var conversation = await conversationRepo.SingleOrDefaultAsync(
                    predicate: c => c.Id == claim.SupportConversationId,
                    include: c => c.Include(x => x.SupportTasks));

                if (conversation == null) throw new NotFoundException("Hội thoại liên quan đến yêu cầu này không tồn tại.");

                var staffId = conversation.ActiveStaffId;

                if (staffId == null)
                    throw new BadRequestException("Không xác định được nhân viên đang phụ trách hội thoại.");

                var currentPendingCount = await conversationRepo.CountAsync(
                    predicate: c => c.ActiveStaffId == staffId &&
                                   c.Status == ConversationStatus.Pending &&
                                   c.Id != conversation.Id);

             
                var statusAfterReject = currentPendingCount >= 5
                    ? ConversationStatus.Warning
                    : ConversationStatus.Pending;

                conversation.Status = statusAfterReject;
                conversation.UpdateDate = DateTime.UtcNow;
                conversation.LastStaffMessageAt = DateTime.UtcNow;

                var pendingReassignTasks = conversation.SupportTasks
                    .Where(t => t.Status == SupportTaskStatus.PendingReassign)
                    .ToList();

                foreach (var task in pendingReassignTasks)
                {
                    task.Status = SupportTaskStatus.InProgress;
                    taskRepo.Update(task);

                    await _taskActionService.CreateTaskActionAsync(new TaskActionRequest
                    {
                        SupportTaskId = task.Id,
                        Action = TaskActionType.Reassigned,
                        ActionById = claim.StaffId,
                        ActionToId = ManagerId,
                        Reason = $"Yêu cầu chuyển giao bị từ chối. Trạng thái hội thoại được đưa về: {statusAfterReject}."
                    });
                }

                conversationRepo.Update(conversation);
                await _unitOfWork.CommitAsync();

                var notificationResponse = new ClaimNotificationResponse
                {
                    ConversationName = conversation.CustomerName,
                    Description = claim.Description ?? "Yêu cầu thay đổi công việc không được phê duyệt.",
                    Status = claim.Status,
                    NewStatus = statusAfterReject.ToString(),
                    Message = "Yêu cầu chuyển công việc đã bị từ chối. Vui lòng tiếp tục hỗ trợ khách hàng."
                };
                await _hubContext.Clients
                .User(staffId.ToString())
                .SendAsync("ReassignRejected", notificationResponse);
            });
        }
    }
}
