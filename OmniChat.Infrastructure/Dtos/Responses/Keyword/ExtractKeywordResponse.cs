using OmniChat.Infrastructure.Dtos.Responses.SupportConversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Keyword
{
    public class ExtractKeywordResponse
    {
        public List<string> Highlights { get; set; } = new();
        public List<IsRecommentOnMesssageResponse> Recommends { get; set; } = new();
    }
}
