using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.CustomerMessage
{
    public class CreateCustomerMessageRequest
    {
        [Required]
        [StringLength(5000)]
        public string Content { get; set; }

        [Required]
        [Range(0, long.MaxValue)]
        public long Timestamp { get; set; }

        public bool KeywordActive { get; set; } = false;

        [Required]
        public Guid CustomerId { get; set; }

        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        public Guid ConversationTicketId { get; set; }
    }
}
