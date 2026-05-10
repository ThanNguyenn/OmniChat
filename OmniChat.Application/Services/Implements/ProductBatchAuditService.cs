using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.ProductBatchAudit;
using OmniChat.Infrastructure.Dtos.Responses.Product;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatchAudit;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = OmniChat.Infrastructure.Models.Action;

namespace OmniChat.Application.Services.Implements;

public class ProductBatchAuditService : BaseService<ProductBatchAuditService>, IProductBatchAuditService
{
    public ProductBatchAuditService(IUnitOfWork<OmniChatDbContext> unitOfWork,
        ILogger<ProductBatchAuditService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, mapper, httpContextAccessor)
    {
    }

    public async Task AddAsync(Guid productBatchId, int quantity, Guid? actionById = null)
    {
        await CreateAudit(productBatchId, quantity, quantity, Action.Enter, actionById);
    }

    public async Task<bool> DeleteBatchAuditAsync(Guid id)
    {
        var repo = _unitOfWork.GetRepository<BatchAudit>();
        return await _unitOfWork.ProcessInTransactionAsync (async () =>
        {
            var audit = await repo.GetByIdAsync(id) ?? throw new NotFoundException("Không tìm thây audit");
            repo.Delete(audit);
            return true;
        });
    }

    public async Task ExportAsync(Guid productBatchId, int quantity, Guid? actionById = null)
    {
        await CreateAudit(productBatchId, 0, quantity, Action.Export, actionById);
    }

    public async Task<PagingResponse<GetAllAuditResponse>> GetAllAuditAsync(Guid? productId, Guid? batchId, Action? action, int pageNumber = 1, int pageSize = 20, string sortBy = "createdate ", bool descending = true)
    {
        var repo = _unitOfWork.GetRepository<BatchAudit>();
        var response = await repo.GetPagingListAsync<GetAllAuditResponse>(
            predicate: ba => (!productId.HasValue || ba.ProductBatch.ProductId == productId) &&
                        (!batchId.HasValue || ba.ProductBatchId == batchId) &&
                        (!action.HasValue || ba.Action == action),
            orderBy: query => OrderBy(query, sortBy, descending),
            include: query => query
                .Include(ba => ba.ActionBy),
            selector: e => _mapper.Map<GetAllAuditResponse>(e)
            );
        return response;
    }
    private static IOrderedQueryable<BatchAudit> OrderBy(IQueryable<BatchAudit> query, string sortBy, bool descending)
    {
        sortBy = sortBy?.Trim().ToLower() ?? "createdate";

        return (sortBy, descending) switch
        {
            ("createdate", false) => query.OrderBy(s => s.CreateDate),
            ("createdate", true) => query.OrderByDescending(s => s.CreateDate),
            ("oldvalue", false) => query.OrderBy(s => s.OldValue),
            ("oldvalue", true) => query.OrderByDescending(s => s.OldValue),
            ("newvalue", false) => query.OrderBy(s => s.NewValue),
            ("newvalue", true) => query.OrderByDescending(s => s.NewValue),
            (_, false) => query.OrderBy(s => s.CreateDate),
            (_, true) => query.OrderByDescending(s => s.CreateDate)
        };
    }

    public async Task<GetDetailByBatchIdResponse> GetDetailByBatchIdAsync(Guid productBatchId)
    {
        var repo = _unitOfWork.GetRepository<BatchAudit>();

        var entity = await repo.GetQueryable(ba => ba.ProductBatchId == productBatchId)
            .Include(ba => ba.ProductBatch)
                .ThenInclude(pb => pb.Product)
                    .ThenInclude(f => f.Brand)
            .Include(ba => ba.ActionBy)
            .FirstOrDefaultAsync();

        return _mapper.Map<GetDetailByBatchIdResponse>(entity);
    }

    public async Task RemoveAsync(Guid productBatchId, int quantity, Guid? actionById = null)
    {
        await CreateAudit(productBatchId, 0, quantity, Action.Remove, actionById);
    }

    public async Task<bool> UpdateBatchAuditAsync(Guid id, UpdateBatchAuditRequest updateBatchAuditRequest)
    {
        var repo = _unitOfWork.GetRepository<BatchAudit>();
        return await _unitOfWork.ProcessInTransactionAsync(async () =>
        {
            var audit = await repo.GetByIdAsync(id) ?? throw new NotFoundException("Không tìm thấy audit");
            _mapper.Map(updateBatchAuditRequest, audit);
            repo.Update(audit);
            return true;
        });
    }

    private async Task CreateAudit(
        Guid batchId,
        int oldValue,
        int newValue,
        Action action,
        Guid? actionById)
    {
        var repo = _unitOfWork.GetRepository<BatchAudit>();

        var audit = new BatchAudit
        {
            ProductBatchId = batchId,
            OldValue = oldValue,
            NewValue = newValue,
            Action = action,
            ActionById = actionById
        };

        await repo.InsertAsync(audit);
    }
}
