using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Application.Utils;
using OmniChat.Infrastructure.Dtos.Requests.PostSaleRequest;
using OmniChat.Infrastructure.Dtos.Responses.PostSaleRequest;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;

namespace OmniChat.Application.Services.Implements;

public class PostSaleRequestService : BaseService<PostSaleRequestService>, IPostSaleRequestService
{
    private readonly IOrderService _orderService;
    public PostSaleRequestService(IUnitOfWork<OmniChatDbContext> unitOfWork, ILogger<PostSaleRequestService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor, IOrderService orderService ) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
        _orderService = orderService;
    }

    public async Task<bool> AcceptPostSaleRequestAsync(Guid id)
    {
        var postSaleRequestRepo = _unitOfWork.GetRepository<PostSaleRequest>();
        var staffRepo = _unitOfWork.GetRepository<Staff>();
        var postSaleRequest = await postSaleRequestRepo.GetQueryable(predicate: q => q.Id == id, include: q => q.Include(x => x.Order)).FirstOrDefaultAsync() ?? throw new NotFoundException($"Không tìm thấy yêu cầu");

        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {

            postSaleRequest.Status = PostSaleRequestStatus.Approved;
            postSaleRequest.ResolvedTime = DateTime.UtcNow;
            var staff = await staffRepo.SingleOrDefaultAsync(predicate: s => s.AccountId == _httpContextAccessor.HttpContext.User.GetUserId());
            postSaleRequest.ResolveById = staff.Id;
            postSaleRequest.Order.Status = postSaleRequest.Type == PostSaleRequestType.Refund ? OrderStatus.RefundApproved : OrderStatus.ReturnApproved;
            postSaleRequestRepo.Update(postSaleRequest);
            switch (postSaleRequest.Type)
            {
                case PostSaleRequestType.Refund:
                    await _orderService.ReturnOrderPaidAsync(postSaleRequest.OrderId, postSaleRequest.RefundAmount ?? 0);
                    break;

                case PostSaleRequestType.Return:
                    await _orderService.ReturnOrderUnpaidAsync(postSaleRequest.OrderId, postSaleRequest.RefundAmount ?? 0);
                    break;

                //case PostSaleRequestType.Cancel:
                //    await _orderService.CancelOrderAsync(postSaleRequest.OrderId);
                //    break;
            }
            return true;
        });
    }

    public async Task<bool> RejectPostSaleRequestAsync(Guid id)
    {
        var postSaleRequestRepo = _unitOfWork.GetRepository<PostSaleRequest>();
        var staffRepo = _unitOfWork.GetRepository<Staff>();
        var postSaleRequest = await postSaleRequestRepo.GetQueryable(predicate: q => q.Id == id, include: q =>q.Include(x => x.Order)).FirstOrDefaultAsync() ?? throw new NotFoundException($"Không tìm thấy yêu cầu");

        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {

            postSaleRequest.Status = PostSaleRequestStatus.Rejected;
            postSaleRequest.ResolvedTime = DateTime.UtcNow;
            var staff = await staffRepo.SingleOrDefaultAsync(predicate: s => s.AccountId == _httpContextAccessor.HttpContext.User.GetUserId());
            postSaleRequest.ResolveById = staff.Id;
            postSaleRequest.Order.Status = postSaleRequest.Type == PostSaleRequestType.Refund ? OrderStatus.RefundRejected : OrderStatus.ReturnRejected;
            postSaleRequestRepo.Update(postSaleRequest);
            return true;
        });
    }

    public async Task<bool> CreatePostSaleRequestAsync(CreatePostSaleRequestRequest request)
    {
        var postSaleRequestRepo = _unitOfWork.GetRepository<PostSaleRequest>();
        var staffRepo = _unitOfWork.GetRepository<Staff>();
        var orderItemRepo = _unitOfWork.GetRepository<OrderItem>();

        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var staff = await staffRepo.SingleOrDefaultAsync( predicate:
                s => s.AccountId == _httpContextAccessor.HttpContext.User.GetUserId());

            if (staff == null)
                throw new NotFoundException("Không tìm thấy nhân viên");

            var postSaleRequest = new PostSaleRequest
            {
                CustomerId = request.CustomerId,
                OrderId = request.OrderId,
                PresentByStaffId = staff.Id,
                Type = request.Type,
                Reason = request.Reason,
                Status = PostSaleRequestStatus.Pending,
                RequestedTime = DateTime.UtcNow
            };

            if (request.PostSaleItems == null || !request.PostSaleItems.Any())
                throw new NotFoundException("Không tìm thấy sản phẩm cần xử lý");

            var orderItemIds = request.PostSaleItems.Select(x => x.OrderItemId).ToList();

            var orderItems = await orderItemRepo.GetListAsync( predicate:
                oi => orderItemIds.Contains(oi.Id));

            var orderItemDict = orderItems.ToDictionary(x => x.Id);

            double totalAmount = 0;
            var postSaleItems = new List<PostSaleItem>();

            foreach (var item in request.PostSaleItems)
            {
                if (!orderItemDict.TryGetValue(item.OrderItemId, out var orderItem))
                    throw new NotFoundException($"Không tìm thấy sản phẩm cần xử lý");

                if (item.Quantity <= 0 || item.Quantity > orderItem.Quantity)
                    throw new BusinessException("Số lượng phải > 0");

                totalAmount += item.Quantity * orderItem.Price;

                postSaleItems.Add(new PostSaleItem
                {
                    PostSaleRequestId = postSaleRequest.Id,
                    OrderItemId = item.OrderItemId,
                    Quantity = item.Quantity
                });
            }

            // Only calculate refund for valid types
            if (request.Type == PostSaleRequestType.Return ||
                request.Type == PostSaleRequestType.Refund)
            {
                postSaleRequest.RefundAmount = totalAmount;
            }

            postSaleRequest.PostSaleItems = postSaleItems;

            await postSaleRequestRepo.InsertAsync(postSaleRequest);

            return true;
        });
    }
    public async Task<bool> DeletePostSaleRequestAsync(Guid id)
    {
        var postSaleRequestRepo = _unitOfWork.GetRepository<PostSaleRequest>();
        var postSaleItemRepo = _unitOfWork.GetRepository<PostSaleItem>();
        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var postSaleRequest = await postSaleRequestRepo.SingleOrDefaultAsync(predicate: p => p.Id == id, include: q => q.Include(p => p.PostSaleItems!));

            if (postSaleRequest == null) throw new NotFoundException($"Không tìm thấy yêu cầu");

            postSaleItemRepo.DeleteRange(postSaleRequest.PostSaleItems!);
            postSaleRequestRepo.Delete(postSaleRequest);
            return true;
        });
    }

    public async Task<GetPostSaleRequestByIdResponse> GetPostSaleRequestByIdAsync(Guid id)
    {
        var postSaleRequestRepo = _unitOfWork.GetRepository<PostSaleRequest>();

        var entity = await postSaleRequestRepo
            .SingleOrDefaultAsync(predicate: p => p.Id == id);

        if (entity == null)
            throw new NotFoundException("Không tìm thấy yêu cầu");

        return _mapper.Map<GetPostSaleRequestByIdResponse>(entity);
    }

    public async Task<PagingResponse<GetPostSaleRequestsResponse>> GetPostSaleRequestsAsync(
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "createddate",
        bool descending = true)
    {
        var postSaleRequestRepo = _unitOfWork.GetRepository<PostSaleRequest>();
        var response = await postSaleRequestRepo.GetPagingListAsync<GetPostSaleRequestsResponse>(
                orderBy: q => OrderBy(q, sortBy, descending),
                include: q => q.Include(x => x.Customer).Include(x => x.PresentByStaff).Include(x => x.PostSaleItems).ThenInclude(i => i.OrderItem).ThenInclude(oi => oi.ProductBatch).ThenInclude(pb => pb.Product),
                selector: e => _mapper.Map<GetPostSaleRequestsResponse>(e),
                page: pageNumber,
                size: pageSize
            );
        return response;
    }

    private static IOrderedQueryable<PostSaleRequest> OrderBy(IQueryable<PostSaleRequest> query, string sortBy, bool descending)
    {
        sortBy = sortBy?.Trim().ToLower() ?? "createdtime";

        return (sortBy, descending) switch
        {
            ("id", false) => query.OrderBy(s => s.Id),
            ("id", true) => query.OrderByDescending(s => s.Id),
            ("createdtime", false) => query.OrderBy(s => s.CreateTime),
            (_, true) => query.OrderByDescending(s => s.CreateTime)
        };
    }



    public Task<bool> UpdatePostSaleRequestAsync(Guid id, UpdatePostSaleRequestRequest request)
    {
        throw new NotImplementedException();
    }
}
