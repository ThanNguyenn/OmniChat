using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.CustomerProfile
{
    public class UpdateCustomerProfileRequest
    {
        [StringLength(100, ErrorMessage = "Tên không được quá 100 ký tự.")]
        public string? CustomerName { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ quá dài.")]
        public string? Address { get; set; }

        [Url(ErrorMessage = "Định dạng Avatar URL không hợp lệ.")]
        public string? AvatarUrl { get; set; }

        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        [RegularExpression(@"^(0[3|5|7|8|9])([0-9]{8})$", ErrorMessage = "Số điện thoại Không hợp Lệ")]
        public string? PhoneNumber { get; set; }
    }
}
