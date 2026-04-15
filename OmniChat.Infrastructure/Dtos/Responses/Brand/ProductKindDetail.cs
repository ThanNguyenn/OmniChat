using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Brand
{
    public class ProductKindDetail
    {
        public string KindName { get; set; }
        public IEnumerable<ProductVolumeDetail> Volumes { get; set; }
    }
}
