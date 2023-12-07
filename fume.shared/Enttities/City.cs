using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fume.shared.Enttities
{
    public class City
    {
        public int Id { get; set; }

        [Display(Name = "Ciudad")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(150, ErrorMessage = "El campo {0} no puede tener mas de {1} Caracteres")]

        public int StateId { get; set; }

        public string Name { get; set; } = null!;

        public State? States { get; set; }
    }
}
