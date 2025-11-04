using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fume.shared.Enttities
{
    public class Category
    {
        public int Id { get; set; }

        [Display(Name = "Categoria")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]

        public string Name { get; set; } = null!;

        public ICollection<ProductCategory>? ProductCategories { get; set; }

        [Display(Name = "Productos")]
        public int ProductCategoriesNumber => ProductCategories == null ? 0 : ProductCategories.Count;

        public ICollection<SubCategory>? SubCategories { get; set; }

        [Display(Name = "Subcategorias")]
        public int SubCategoriesNumber => SubCategories == null ? 0 : SubCategories.Count;

        [Display(Name = "Imagen")]
        public byte[]? Image { get; set; }

        [Display(Name = "Imagen")]
        public string ImageString => Image == null || Image.Length == 0 ? string.Empty : Convert.ToBase64String(Image);

        [Display(Name = "URL de Imagen")]
        public string? ImageUrl { get; set; }
    }
}
