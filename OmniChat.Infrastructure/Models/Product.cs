using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        public string ImageUrl { get; set; }

        public string Name { get; set; } 

        public PackagingType ProductPackagingType { get; set; }

        public double VolumeMl { get; set; }

        public string Description { get; set; }

        public bool? IsActive { get; set; }

        public Guid BrandId { get; set; }

        public virtual Brand Brand { get; set; }

        public double Price { get; set; }

        public string Code { get; set; } 

        public int Quantity { get; set; }

        public DateTime CreateDate { get; set; }

        public int LifeSpan { get; set; }

        public virtual ICollection<ProductBatch> ProductBatches { get; set; } = new List<ProductBatch>();
    }

    public enum PackagingType
    {
        Bottle = 0,
        Carton = 1,
    }
}
