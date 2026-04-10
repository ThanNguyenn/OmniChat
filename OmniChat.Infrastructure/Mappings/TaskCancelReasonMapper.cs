using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.TaskCancelReason;
using OmniChat.Infrastructure.Dtos.Responses.TaskCancelReason;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class TaskCancelReasonMapper : Profile
    {
        public TaskCancelReasonMapper()
        {
            CreateMap<TaskCancelReasonRequest, TaskCancelReason>()
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(_ => DateTime.UtcNow));

            CreateMap<TaskCancelReason, TaskCancelReasonResponse>();
        }
    }
}
