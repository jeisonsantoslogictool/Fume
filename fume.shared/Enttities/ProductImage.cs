using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace fume.shared.Enttities
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        [Display(Name = "Foto")]
        public byte[] Imagefile { get; set; } = null!;

        [Display(Name = "URL de Imagen")]
        public string? ImageUrl { get; set; }
    }
}
