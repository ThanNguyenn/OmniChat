using AutoMapper;
using OmniChat.Infrastructure.Dtos.Requests.CustomerMessage;
using OmniChat.Infrastructure.Dtos.Responses.CustomerMessage;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Mappings
{
    public class CustomerMessageMapper : Profile
    {
        public CustomerMessageMapper()
        {
            CreateMap<CreateCustomerMessageRequest, CustomerMessage>();
            CreateMap<CustomerMessage, CreateCustomerMessageResponse>();
        }
    }
}
