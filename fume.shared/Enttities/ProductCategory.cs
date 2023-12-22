using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fume.shared.Enttities
{
    public class ProductCategory
    {
        public int Id { get; set; }

        public Product Products { get; set; } = null!;

        public int ProductId { get; set; }

        public Category Category { get; set; } = null!;

        public int CategoryId { get; set; }
    }
}
