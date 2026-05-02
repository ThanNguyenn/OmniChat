using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Claim
{
    public class CreateClaimRequest
    {
        [Required(ErrorMessage = "Mô tả không được để trống")]
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Lý do không được để trống")]
        public string Reason { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn nhân viên")]
        public Guid StaffId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại khiếu nại")]
        public Guid ClaimTypeId { get; set; }

        public Guid? SupportConversationId { get; set; }
    }
}
