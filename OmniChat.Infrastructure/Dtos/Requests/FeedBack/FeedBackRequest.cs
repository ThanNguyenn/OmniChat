using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.FeedBack
{
    public class FeedBackRequest
    {
        public string Content { get; set; }

        public string CustomerEmail { get; set; }

        public int Rating { get; set; } 

    }
}
