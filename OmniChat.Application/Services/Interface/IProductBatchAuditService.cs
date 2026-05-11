
using OmniChat.Infrastructure.Dtos.Requests.ProductBatchAudit;
using OmniChat.Infrastructure.Dtos.Responses.ProductBatchAudit;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Action = OmniChat.Infrastructure.Models.Action;

namespace OmniChat.Application.Services.Interface;

public interface IProductBatchAuditService
{
    Task AddAsync(Guid productBatchId, int oldValue, int newValue, Guid? actionById = null);

    Task ExportAsync(Guid productBatchId, int oldValue, int newValue, Guid? actionById = null);

    Task RemoveAsync(Guid productBatchId, int oldValue, int newValue, Guid? actionById = null);

    Task<bool> DeleteBatchAuditAsync(Guid id);

    Task<bool> UpdateBatchAuditAsync(Guid id, UpdateBatchAuditRequest updateBatchAuditRequest);

    Task<GetDetailByBatchIdResponse> GetDetailByBatchIdAsync(Guid productBatchId);

    Task<PagingResponse<GetAllAuditResponse>> GetAllAuditAsync(
        Guid? productId,
        Guid? batchId,
        Action? action,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "createdate ",
        bool descending = true);
}
