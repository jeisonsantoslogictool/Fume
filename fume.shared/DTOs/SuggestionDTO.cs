using System.ComponentModel.DataAnnotations;

namespace fume.shared.DTOs
{
    public class SuggestionDTO
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        [MaxLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "El mensaje es requerido")]
        [MaxLength(1000, ErrorMessage = "El mensaje no puede exceder 1000 caracteres")]
        public string Message { get; set; } = null!;
    }
}
