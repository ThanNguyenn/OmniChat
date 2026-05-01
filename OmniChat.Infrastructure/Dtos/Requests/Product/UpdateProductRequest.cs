using Microsoft.AspNetCore.Http;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Product;

public class UpdateProductRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    [Range(1d, double.MaxValue, ErrorMessage = "Price phải >= 1")]
    public double? Price { get; set; }

}
