using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.ClaimType
{
    public class ClaimTypeRequest
    {
        [Required(ErrorMessage = "Tên loại là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên loại không được vượt quá 200 ký tự")]
        [RegularExpression(@"^(?=.*\S)[a-zA-Z0-9 _-]+$",
        ErrorMessage = "Tên loại không được chứa ký tự đặc biệt hoặc chỉ có khoảng trắng")]
        public string TypeName { get; set; }
    }
}
