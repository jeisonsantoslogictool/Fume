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

        [Display(Name = "Icono")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres.")]
        public string? Icon { get; set; }

        // Propiedad calculada para saber si tiene imagen (sin traer los bytes)
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool HasImage { get; set; }

        // Propiedad para invalidar caché de imágenes (solo cliente, no se guarda)
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public long ImageModifiedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Propiedad para controlar el estado expandido en el NavMenu (solo cliente)
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IsExpanded { get; set; }
    }
}
