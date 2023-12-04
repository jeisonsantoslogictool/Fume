using System.ComponentModel.DataAnnotations;

namespace fume.shared.Enttities
{
    public class Country
    {
        public int Id { get; set; }

        [Display(Name = "Pais")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(150, ErrorMessage = "El campo {0} no puede tener mas de {1} Caracteres")]
        public string Name { get; set; } = null!;

    }
}
