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
        [Required(ErrorMessage = "Mã cuộc hội thoại là bắt buộc")]
        public Guid SupportConversationId { get; set; }

        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        public Guid StaffId { get; set; }


        [Required(ErrorMessage = "Nội dung tin nhắn không được để trống")]
        [MinLength(1, ErrorMessage = "Nội dung tin nhắn phải có ít nhất 1 ký tự")]
        [MaxLength(2000, ErrorMessage = "Nội dung tin nhắn không được vượt quá 2000 ký tự")]
        public string Content { get; set; }

    }
}
