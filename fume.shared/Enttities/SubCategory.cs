using System.ComponentModel.DataAnnotations;

namespace fume.shared.Enttities
{
    public class SubCategory
    {
        public int Id { get; set; }

        [Display(Name = "Subcategoria")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Name { get; set; } = null!;

        [Display(Name = "Categoria")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;

        public ICollection<ProductSubCategory>? ProductSubCategories { get; set; }

        [Display(Name = "Productos")]
        public int ProductSubCategoriesNumber => ProductSubCategories == null ? 0 : ProductSubCategories.Count;

        [Display(Name = "Imagen")]
        public byte[]? Image { get; set; }

        [Display(Name = "Imagen")]
        public string ImageString => Image == null || Image.Length == 0 ? string.Empty : Convert.ToBase64String(Image);

        [Display(Name = "URL de Imagen")]
        public string? ImageUrl { get; set; }

        // Propiedad calculada para saber si tiene imagen (sin traer los bytes)
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool HasImage { get; set; }

        // Propiedad para invalidar caché de imágenes (solo cliente, no se guarda)
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public long ImageModifiedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
