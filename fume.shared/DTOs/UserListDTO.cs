using fume.shared.Enttities;
using fume.shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace fume.shared.DTOs
{
    public class UserListDTO
    {
        public string Id { get; set; } = null!;

        [Display(Name = "Documento")]
        public string Document { get; set; } = null!;

        [Display(Name = "Nombres")]
        public string FirstName { get; set; } = null!;

        [Display(Name = "Apellidos")]
        public string LastName { get; set; } = null!;

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Teléfono")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Dirección")]
        public string Address { get; set; } = null!;

        [Display(Name = "Tipo de usuario")]
        public UserType UserType { get; set; }

        [Display(Name = "Ciudad")]
        public int CityId { get; set; }

        public City? City { get; set; }

        public bool HasPhoto { get; set; }

        [Display(Name = "Usuario")]
        public string FullName => $"{FirstName} {LastName}";
    }
}
