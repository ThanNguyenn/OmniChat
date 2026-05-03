using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Product;

public class UpdateProductImageRequest
{
    [Required(ErrorMessage = "File không hỗ trợ")]
    public IFormFile? Image { get; set; }
}
