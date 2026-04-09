using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.TaskAction;
using OmniChat.Infrastructure.Dtos.Responses.TaskAction;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class TaskActionMapper : Profile
    {
        public TaskActionMapper()
        {
            CreateMap<TaskActionRequest, TaskAction>();
            CreateMap<TaskAction, TaskActionResponse>();
        }
    }
}
