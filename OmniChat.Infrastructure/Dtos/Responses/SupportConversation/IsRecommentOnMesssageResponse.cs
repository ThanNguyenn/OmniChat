using OmniChat.Infrastructure.Dtos.Responses.Keyword;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.SupportConversation
{
    public class IsRecommentOnMesssageResponse
    {
        public IRecommendData? Data { get; set; }

        public RecommendType RecommendType { get; set; }
    }

    public enum RecommendType
    {
        SearchOrderHistory = 0,
        SearchProduct = 1,
        SearchCustomerInfo = 2,
    }
}
