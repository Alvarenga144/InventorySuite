namespace Inventory.Web.Models.ViewModels
{
    public class AuthResponseViewModel
    {
        public string Token { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }
}

