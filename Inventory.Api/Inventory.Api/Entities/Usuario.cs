using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.Entities
{
    public class Usuario : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relación con ventas
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
