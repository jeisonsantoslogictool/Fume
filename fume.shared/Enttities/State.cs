using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fume.shared.Enttities
{
    public class State
    {
        public int Id { get; set; }
        [Display(Name = "Estados/Provincias")]
        [Required(ErrorMessage = "El campo{0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener mas de {1} caracteres")]

        public int CountryId { get; set; }

        public string Name { get; set; } = null!;

        public Country? country { get; set; }

        public ICollection<City>? Cities { get; set; }

        public int CitiesNumber => Cities == null ? 0 : Cities.Count;

    }
}
