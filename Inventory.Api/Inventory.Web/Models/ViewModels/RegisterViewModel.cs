using System.ComponentModel.DataAnnotations;

namespace Inventory.Web.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es requerido")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;
    }
}

