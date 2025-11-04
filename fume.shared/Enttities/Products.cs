using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace fume.shared.Enttities
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "Nombre")]
        [MaxLength(50, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public string Name { get; set; } = null!;

        [DataType(DataType.MultilineText)]
        [Display(Name = "Descripción")]
        [MaxLength(500, ErrorMessage = "El campo {0} debe tener máximo {1} caractéres.")]
        public string Description { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        [Display(Name = "Precio")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public decimal Price { get; set; }

        [DisplayFormat(DataFormatString = "{0:N2}")]
        [Display(Name = "Inventario")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public float Stock { get; set; }

        public ICollection<ProductCategory>? ProductCategories { get; set; }

        [Display(Name = "Categorías")]
        public int ProductCategoriesNumber => ProductCategories == null ? 0 : ProductCategories.Count;

        public ICollection<ProductSubCategory>? ProductSubCategories { get; set; }

        [Display(Name = "Subcategorías")]
        public int ProductSubCategoriesNumber => ProductSubCategories == null ? 0 : ProductSubCategories.Count;

        public ICollection<ProductImage>? ProductImages { get; set; }

        [Display(Name = "Imágenes")]
        public int ProductImagesNumber => ProductImages == null ? 0 : ProductImages.Count;

        [Display(Name = "Imagén")]
        public string MainImage => ProductImages == null || !ProductImages.Any() || ProductImages.First().Imagefile == null
            ? string.Empty
            : Convert.ToBase64String(ProductImages.First().Imagefile);

        [Display(Name = "Imágenes Base64")]
        public List<string> ProductImagesString => ProductImages == null || !ProductImages.Any()
            ? new List<string>()
            : ProductImages.Where(x => x.Imagefile != null).Select(x => Convert.ToBase64String(x.Imagefile)).ToList();





    }
}
