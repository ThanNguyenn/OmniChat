using Microsoft.AspNetCore.Http;
using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Product;

public class UpdateProductRequest
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Brand { get; set; }

    public double? Price { get; set; }

}
