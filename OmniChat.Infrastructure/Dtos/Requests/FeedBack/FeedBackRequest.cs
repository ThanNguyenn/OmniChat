using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.FeedBack
{
    public class FeedBackRequest
    {
        [Required(ErrorMessage = "Nội dung phản hồi không được để trống.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Nội dung phải từ 10 đến 1000 ký tự.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Email khách hàng không được để trống.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng.")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng đánh giá số sao.")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải nằm trong khoảng từ 1 đến 5 sao.")]
        public int Rating { get; set; } 

    }
}
