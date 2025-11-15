using System.ComponentModel.DataAnnotations;

namespace fume.shared.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; } = null!;
    }
}
