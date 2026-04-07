using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Utils;
using OmniChat.Infrastructure.Dtos.Requests.Staff;
using OmniChat.Infrastructure.Dtos.Requests.SupportTask;
using OmniChat.Infrastructure.Dtos.Responses.Staff;
using OmniChat.Infrastructure.Dtos.Responses.SupportTask;
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
            //commit to get account id for staff
            await _unitOfWork.CommitAsync();

            staff.AccountId = account.Id;
            await staffRepo.InsertAsync(staff);
            //commit to get staff id for staff intent type
            await _unitOfWork.CommitAsync();
            if (createStaffRequest.StaffIntentTypes != null && createStaffRequest.StaffIntentTypes.Any())
            {
                await AssignIntentToStaffAsync(staff.Id, createStaffRequest.StaffIntentTypes);
            }
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
            await SyncStaffIntentsAsync(existingStaff.Id, updateStaffRequest.StaffIntentTypes);
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


    public async Task<PagingResponse<GetStaffsResponse>> GetStaffsAsync(
        string? search = null,
        IEnumerable<Guid>? departmentIds = null,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "id",
        bool descending = false)
    {
        var staffRepository = _unitOfWork.GetRepository<Staff>();

        var response = await staffRepository.GetPagingListAsync<GetStaffsResponse>(
            predicate: s =>
                s.IsActive == true &&
                (departmentIds == null || !departmentIds.Any() ||
                    departmentIds.All(id =>
                        s.StaffIntentTypes.Any(sit => sit.IntentTypeId == id)
                    )
                ) &&
                (string.IsNullOrEmpty(search) ||
                    s.Name.Contains(search) ||
                    s.Email.Contains(search) ||
                    s.Phone.Contains(search)
                ),

            orderBy: q => OrderBy(q, sortBy, descending),
            selector: e => _mapper.Map<GetStaffsResponse>(e),
            page: pageNumber,
            size: pageSize
        );

        return response;
    }

    private static IOrderedQueryable<Staff> OrderBy(IQueryable<Staff> query, string sortBy, bool descending)
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

    private async Task SyncStaffIntentsAsync(Guid staffId, IEnumerable<AssignStaffToIntentTypeRequest> requests)
    {
        var staffIntentTypeRepo = _unitOfWork.GetRepository<StaffIntentType>();
        var intentTypeRepo = _unitOfWork.GetRepository<IntentType>();

        var newIntentIds = requests?.Select(r => r.IntentId).Distinct().ToList() ?? new List<Guid>();
        if (newIntentIds.Any())
        {
            var validIntents = await intentTypeRepo.GetListAsync(
                predicate: it => newIntentIds.Contains(it.Id) && it.IsActive != false);

            if (validIntents.Count != newIntentIds.Count)
            {
                throw new NotFoundException("One or more intent types not found or inactive.");
            }
        }

        var existingAssignments = await staffIntentTypeRepo.GetListAsync(
            predicate: sit => sit.StaffId == staffId);

        var existingIntentIds = existingAssignments.Select(x => x.IntentTypeId).ToList();

        var toAdd = newIntentIds.Except(existingIntentIds).ToList();
        var toRemove = existingAssignments
            .Where(x => !newIntentIds.Contains(x.IntentTypeId))
            .ToList();

        if (toRemove.Any())
        {
            staffIntentTypeRepo.DeleteRange(toRemove);
        }

        if (toAdd.Any())
        {
            var newAssignments = toAdd.Select(id => new StaffIntentType
            {
                StaffId = staffId,
                IntentTypeId = id
            }).ToList();

            await staffIntentTypeRepo.InsertRangeAsync(newAssignments);
        }
    }


    public async Task<StaffDassboardResponse> GetStaffDassboardByIdAsync(Guid staffId)
    {
        var now = DateTime.UtcNow;
        var taskRepo = _unitOfWork.GetRepository<SupportTask>();
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var performanceRepo = _unitOfWork.GetRepository<StaffPerformance>();

        // Lấy performance current month
        var currentPerformance = await performanceRepo.SingleOrDefaultAsync(
            predicate: x => x.StaffId == staffId &&
                            x.FromTime <= now &&
                            x.ToTime >= now
        );

        var totalCreateOrder = await orderRepo.CountAsync(
            o => o.CreatorId == staffId &&
                 o.IsDeleted != true
        );

        return new StaffDassboardResponse
        {
            TotalDoneTask = currentPerformance?.TaskCompleted ?? 0,
            TotalCreateOrder = totalCreateOrder,
            AfferageResolveTime = currentPerformance?.AvgTaskHandleTime / 60.0 ?? 0,
            StaffPerformance = CalculatePerformanceScore(currentPerformance)
        };
    }

    private double CalculatePerformanceScore(StaffPerformance? performance)
    {
        if (performance == null) return 0;

        var total = performance.TaskCompleted + performance.CancelledCount + performance.ReassignmentCount;

        if (total == 0) return 0;

        return Math.Round((double)performance.TaskCompleted / total * 100, 2);
    }

    public async Task<PagingResponse<StaffSupportTaskResponse>> GetStaffTasksAsync(Guid staffId, StaffTaskFilterRequest request)
    {
        var repo = _unitOfWork.GetRepository<SupportTask>();
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
        var fromDate = request.FromDate?.Date;
        var toDate = request.ToDate?.Date.AddDays(1);

        var result = await repo.GetPagingListAsync(
            predicate: t =>
                t.CurrentAssignedStaffId == staffId &&
                t.Status == SupportTaskStatus.Done &&
                t.CompleteDate != null &&
                (!fromDate.HasValue || t.CompleteDate >= fromDate) &&
                (!toDate.HasValue || t.CompleteDate < toDate) &&
                (!request.IntentTypeId.HasValue || t.IntentTypeId == request.IntentTypeId),
            orderBy: q => q.OrderByDescending(t => t.CompleteDate),
            include: q => q
                .Include(t => t.IntentType)
                .Include(t => t.SupportConversation),
            page: page,
            size: pageSize
        );

        return new PagingResponse<StaffSupportTaskResponse>
        {
            Items = _mapper.Map<List<StaffSupportTaskResponse>>(result.Items),
            Meta = new PaginationMeta
            {
                TotalItems = result.Meta.TotalItems,
                TotalPages = result.Meta.TotalPages,
                CurrentPage = page,
                PageSize = pageSize
            }
        };
    }

    public async Task AssignShipperOrderAsync(Guid shipperId, Guid orderId)
    {
        var orderRepo = _unitOfWork.GetRepository<Order>();
        var staffRepo = _unitOfWork.GetRepository<Staff>();

        var order = await orderRepo.GetByIdAsync(orderId);
        if (order == null || order.IsDeleted == true)
        {
            throw new NotFoundException("Order not found.");
        }

        var staff = await staffRepo.SingleOrDefaultAsync(
            predicate: s => s.Id == shipperId,
            include: q => q.Include(s => s.Account)
        );

        if (staff == null)
        {
            throw new NotFoundException("Staff not found.");
        }


        if (staff.Account.Role.Name != "Shipper")
        {
            throw new BadRequestException("Staff is not a shipper.");
        }


        if (staff.IsActive != true)
        {
            throw new BadRequestException("Staff is not active.");
        }


        if (staff.Status != StaffStatus.Online)
        {
            throw new BadRequestException("Shipper is not online.");
        }


        if (order.DriverId != null)
        {
            throw new BadRequestException("Order already has a shipper.");
        }

        order.DriverId = shipperId;

        orderRepo.Update(order);
        await _unitOfWork.CommitAsync();
    }

    public async Task<PagingResponse<ShipperResposne>> GetShippersAsync(int pageIndex = 1, int pageSize = 10)
    {
        var staffRepo = _unitOfWork.GetRepository<Staff>();

        var query = staffRepo.GetQueryable()
            .Where(s => s.Account.Role.Name == "Shipper" && s.IsActive == true)
            .Select(s => new ShipperResposne
            {
                ShipperName = s.Name,
                ShipperPhone = s.Phone,
                ShipperStatus = s.Status,

                TotalPendingOrders = s.OrdersAsDriver
                    .Count(o => o.DeliveryStatus == DeliveryStatus.Pending),

                TotalOrderShipNow = s.OrdersAsDriver
                    .Count(o => o.Status == OrderStatus.Shipped
                             && o.DeliveryStatus == DeliveryStatus.Pending),

                TotalOrderShipped = s.OrdersAsDriver
                    .Count(o => o.DeliveryStatus == DeliveryStatus.Completed)
            });

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        return new PagingResponse<ShipperResposne>
        {
            Items = items,
            Meta = new PaginationMeta
            {
                CurrentPage = pageIndex,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            }
        };
    }

    public async Task<ShipperResposne> GetShipperByShipperIdAsync(Guid shipperId)
    {
        var staffRepo = _unitOfWork.GetRepository<Staff>();

        var shipper = await staffRepo.GetQueryable()
            .Where(s => s.Id == shipperId
                     && s.IsActive == true
                     && s.Account != null
                     && s.Account.Role != null
                     && s.Account.Role.Name == "Shipper")
            .Select(s => new ShipperResposne
            {
                ShipperName = s.Name,
                ShipperPhone = s.Phone,
                ShipperStatus = s.Status,

                TotalPendingOrders = s.OrdersAsDriver
                    .Count(o => o.DeliveryStatus == DeliveryStatus.Pending),

                TotalOrderShipNow = s.OrdersAsDriver
                    .Count(o => o.Status == OrderStatus.Shipped
                             && o.DeliveryStatus == DeliveryStatus.Pending),

                TotalOrderShipped = s.OrdersAsDriver
                    .Count(o => o.DeliveryStatus == DeliveryStatus.Completed)
            })
            .FirstOrDefaultAsync();

        if (shipper == null)
        {
            throw new NotFoundException("Shipper not found");
        }

        return shipper;
    }

}
