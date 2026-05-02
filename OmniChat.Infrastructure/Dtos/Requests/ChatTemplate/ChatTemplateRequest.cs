using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.ChatTemplate
{
    public class ChatTemplateRequest
    {
        [Required(ErrorMessage = "Mã Mẫu Chat là bắt buộc")]
        [RegularExpression(@"^[A-Za-z]{1,5}\d{1,3}$",
        ErrorMessage = "Mã Mẫu Chat không hợp lệ (Định dạng đúng: 1-5 chữ cái + 1-3 chữ số)")]
        public string Code { get; set; }

        [Required(ErrorMessage = "Nội dung Mẫu Chat là bắt buộc")]
        [StringLength(500, ErrorMessage = "Nội dung không quá 500 ký tự")]
        public string Content { get; set; }
    }
}
