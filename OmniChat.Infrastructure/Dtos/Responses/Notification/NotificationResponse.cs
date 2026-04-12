using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Notification
{
    public class NotificationResponse
    {
        public string Message { get; set; }

        public string CustomerName { get; set; }

        public string ImageUrl { get; set; }

        public string ProviderName { get; set; }

        public long TimeStamp  { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
