using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.SupportConversation
{
    public class CountPendingConverRepsonse
    {
        public string ProviderName { get; set; }

        public int Total { get; set; }
    }
}
