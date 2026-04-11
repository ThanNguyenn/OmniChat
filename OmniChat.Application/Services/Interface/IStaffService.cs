using OmniChat.Infrastructure.Dtos.Requests.Staff;
using OmniChat.Infrastructure.Dtos.Requests.SupportTask;
using OmniChat.Infrastructure.Dtos.Responses.Staff;
using OmniChat.Infrastructure.Dtos.Responses.SupportTask;
using OmniChat.Infrastructure.Metadatas;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IStaffService
{
    Task<bool> CreateStaffAsync(CreateStaffRequest createStaffRequest);

    Task<bool> UpdateStaffAsync(Guid StaffId, UpdateStaffRequest updateStaffRequest);

    Task<bool> DeleteStaffAsync(Guid StaffId);

    public Task<StaffDassboardResponse> GetStaffDassboardByIdAsync(Guid staffId);

    public  Task<PagingResponse<StaffSupportTaskResponse>> GetStaffTasksAsync(Guid staffId,StaffTaskFilterRequest request = null);

    Task<PagingResponse<GetStaffsResponse>> GetStaffsAsync(string? search = null, IEnumerable<Guid>? deparmentIds = null, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false);

    Task<bool> AssignIntentToStaffAsync(Guid staffId, IEnumerable<AssignStaffToIntentTypeRequest> requests);

    Task<bool> UnassignIntentFromStaffAsync(Guid staffId, AssignStaffToIntentTypeRequest unassignStaffFromIntentTypeRequest);

    public Task AssignShipperOrderAsync(Guid shipperId, Guid orderId);

    public Task<PagingResponse<ShipperResposne>> GetShippersAsync(int pageIndex = 1, int pageSize = 10);

    public  Task<ShipperResposne> GetShipperByShipperIdAsync(Guid shipperId);

    public  Task<ShipperDashboardResponse> GetShipperDashboardAsync();
}
