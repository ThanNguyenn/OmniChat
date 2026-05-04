using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Provider
{
    public class CreateProviderRequest
    {
        [Required(ErrorMessage = "Tên kênh chat  là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên kênh chat không được vượt quá 200 ký tự")]
        [RegularExpression(@"^(?=.*\S)[a-zA-Z0-9 _-]+$",
      ErrorMessage = "Tên kênh chat không được để trống hoặc chứa ký tự đặc biệt")]
        public string ProviderName { get; set; }
    }
}
