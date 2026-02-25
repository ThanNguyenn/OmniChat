using OmniChat.Infrastructure.Dtos.Requests.Staff;
using OmniChat.Infrastructure.Dtos.Responses.Staff;
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

    Task<PagingResponse<GetStaffsResponse>> GetStaffsAsync(Guid deparmentId, int pageNumber = 1, int pageSize = 20, string sortBy = "id", bool descending = false);

}
