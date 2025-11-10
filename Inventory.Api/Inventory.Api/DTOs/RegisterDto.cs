using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MaxLength(100)]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es requerido")]
        [MaxLength(100)]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = null!;
    }
}

