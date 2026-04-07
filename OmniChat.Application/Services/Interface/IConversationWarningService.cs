using OmniChat.Infrastructure.Dtos.Responses.WarningConversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface
{
    public interface IConversationWarningService
    {
        public  Task<IEnumerable<WarningDetailRepsone>> GetAllWarningsAsync(bool? isReviewed = null);

        public  Task<WarningDetailRepsone> GetWarningByIdAsync(Guid id);
    }
}
