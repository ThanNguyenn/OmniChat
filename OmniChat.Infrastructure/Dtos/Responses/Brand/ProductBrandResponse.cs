using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Brand
{
    public class ProductBrandResponse
    {
        public int TotalProduct { get; set; }
        public IEnumerable<ProductKindDetail> ProductKinds { get; set; }
    }
}
