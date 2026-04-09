using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.ChatTemplate;
using OmniChat.Infrastructure.Dtos.Responses.ChatTemplate;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class ChatTemplateMapper : Profile
    {
        public ChatTemplateMapper()
        {
            CreateMap<ChatTemplateRequest, ChatTemplate>().ReverseMap();
            CreateMap<ChatTemplate, ChatTemplateResponse>();
        }
    }
}
