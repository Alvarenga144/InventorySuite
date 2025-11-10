using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Api.Entities
{
    public class Producto
    {
        [Key]
        [Column("idpro")]
        public int IdPro { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("producto")]
        public string ProductoNombre { get; set; } = null!;

        [Precision(18, 2)]
        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public ICollection<DetalleVenta> DetalleVentas { get; set; } =  new List<DetalleVenta>();
    }
}
