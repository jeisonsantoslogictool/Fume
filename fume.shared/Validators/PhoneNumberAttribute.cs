using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace fume.shared.Validators
{
    public class PhoneNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult("El número de teléfono es obligatorio.");
            }

            var phoneNumber = value.ToString();

            // Remover todos los caracteres que no sean dígitos
            var digitsOnly = Regex.Replace(phoneNumber, @"\D", "");

            // Si comienza con "1", removerlo
            if (digitsOnly.StartsWith("1"))
            {
                digitsOnly = digitsOnly.Substring(1);
            }

            // Validar que tenga exactamente 10 dígitos
            if (digitsOnly.Length != 10)
            {
                return new ValidationResult("El número de teléfono debe tener exactamente 10 dígitos después del +1. (Ej: +1 809-509-9999)");
            }

            // Validar que comience con 809, 829 o 849 (códigos de área válidos en RD)
            if (!digitsOnly.StartsWith("809") &&
                !digitsOnly.StartsWith("829") &&
                !digitsOnly.StartsWith("849"))
            {
                return new ValidationResult("El número de teléfono debe comenzar con 809, 829 o 849. (Ej: +1 809-509-9999)");
            }

            return ValidationResult.Success!;
        }
    }
}
