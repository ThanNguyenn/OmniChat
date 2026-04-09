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
        var postSaleRequest = await postSaleRequestRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Request {id} not found");

        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {

            postSaleRequest.Status = PostSaleRequestStatus.Approved;
            postSaleRequest.ResolvedTime = DateTime.UtcNow;
            var staff = await staffRepo.SingleOrDefaultAsync(predicate: s => s.AccountId == _httpContextAccessor.HttpContext.User.GetUserId());
            postSaleRequest.ResolveById = staff.Id;
            postSaleRequestRepo.Update(postSaleRequest);
            switch (postSaleRequest.Type)
            {
                case PostSaleRequestType.Refund:
                    await _orderService.ReturnOrderPaidAsync(postSaleRequest.OrderId, postSaleRequest.RefundAmount ?? 0);
                    break;

                case PostSaleRequestType.Return:
                    await _orderService.ReturnOrderUnpaidAsync(postSaleRequest.OrderId, postSaleRequest.RefundAmount ?? 0);
                    break;

                case PostSaleRequestType.Cancel:
                    await _orderService.CancelOrderAsync(postSaleRequest.OrderId);
                    break;
            }
            return true;
        });
    }

    public async Task<bool> RejectPostSaleRequestAsync(Guid id)
    {
        var postSaleRequestRepo = _unitOfWork.GetRepository<PostSaleRequest>();
        var staffRepo = _unitOfWork.GetRepository<Staff>();
        var postSaleRequest = await postSaleRequestRepo.GetByIdAsync(id) ?? throw new NotFoundException($"Request {id} not found");

        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {

            postSaleRequest.Status = PostSaleRequestStatus.Rejected;
            postSaleRequest.ResolvedTime = DateTime.UtcNow;
            var staff = await staffRepo.SingleOrDefaultAsync(predicate: s => s.AccountId == _httpContextAccessor.HttpContext.User.GetUserId());
            postSaleRequest.ResolveById = staff.Id;
            postSaleRequestRepo.Update(postSaleRequest);
            return true;
        });
    }

    public async Task<bool> CreatePostSaleRequestAsync(CreatePostSaleRequestRequest request)
    {
        var postSaleRequestRepo = _unitOfWork.GetRepository<PostSaleRequest>();
        var staffRepo = _unitOfWork.GetRepository<Staff>();
        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var postSaleRequest = _mapper.Map<PostSaleRequest>(request);
            var staff = await staffRepo.SingleOrDefaultAsync(predicate: s => s.AccountId == _httpContextAccessor.HttpContext.User.GetUserId());
            postSaleRequest.PresentByStaffId = staff.Id;

            postSaleRequest.Status = PostSaleRequestStatus.Pending;

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

            if (postSaleRequest == null) throw new NotFoundException($"Request {id} not found");

            postSaleItemRepo.DeleteRange(postSaleRequest.PostSaleItems!);
            postSaleRequestRepo.Delete(postSaleRequest);
            return true;
        });
    }

    public async Task<GetPostSaleRequestByIdResponse> GetPostSaleRequestByIdAsync(Guid id)
    {
        var postSaleRequestRepo = _unitOfWork.GetRepository<PostSaleRequest>();

        var query = postSaleRequestRepo.GetQueryable();

        var response = await query
            .Where(p => p.Id == id)
            .ProjectTo<GetPostSaleRequestByIdResponse>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();

        if (response == null) throw new NotFoundException($"Request {id} not found");

        return response;
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
