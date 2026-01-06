using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.SupportStaffMessage
{
    public class CreateSupportStaffMessageRequest
    {
        [Required(ErrorMessage = "SupportConversationId is required")]
        public Guid SupportConversationId { get; set; }

        [Required(ErrorMessage = "StaffId is required")]
        public Guid StaffId { get; set; }


        [Required(ErrorMessage = "Content is required")]
        [MinLength(1, ErrorMessage = "Content cannot be empty")]
        [MaxLength(2000, ErrorMessage = "Content maximum length is 2000 characters")]
        public string Content { get; set; }

    }
}
