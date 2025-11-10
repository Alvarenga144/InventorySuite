using System.ComponentModel.DataAnnotations;

namespace Inventory.Web.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = null!;
    }
}

