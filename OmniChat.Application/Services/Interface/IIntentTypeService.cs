using OmniChat.Infrastructure.Dtos.Responses.IntentType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IIntentTypeService
    {
        public  Task<IEnumerable<GetsIntentTypeResponse>> GetIntentTypesAsync();
    }
}
