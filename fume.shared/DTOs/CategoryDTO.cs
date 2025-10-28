using System.ComponentModel.DataAnnotations;

namespace fume.shared.DTOs
{
    public class CategoryDTO
    {
        public int Id { get; set; }

        [Display(Name = "Categoria")]
        [MaxLength(100, ErrorMessage = "El campo {0} debe tener maximo {1} caracteres.")]
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Name { get; set; } = null!;

        [Display(Name = "Imagen")]
        public string? ImageString { get; set; }
    }
}
