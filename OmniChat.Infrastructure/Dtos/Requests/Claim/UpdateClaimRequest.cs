using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Claim
{
    public class UpdateClaimRequest
    {
        [Required(ErrorMessage = "Mô tả không được để trống")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Mô tả phải từ 10 đến 500 ký tự")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Lý do là bắt buộc")]
        [MaxLength(200, ErrorMessage = "Lý do không được vượt quá 200 ký tự")]
        public string Reason { get; set; }
    }
}
