using System.ComponentModel.DataAnnotations;

namespace fume.shared.DTOs
{
    public class SubCategoryDTO
    {
        public int Id { get; set; }

        [Display(Name = "Subcategoria")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Name { get; set; } = null!;

        [Display(Name = "Categoria")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public int CategoryId { get; set; }

        [Display(Name = "Imagen")]
        public string? ImageString { get; set; }
    }
}
