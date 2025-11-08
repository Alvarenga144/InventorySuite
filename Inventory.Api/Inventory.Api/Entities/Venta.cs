using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Api.Entities
{
    public class Venta
    {
        [Key]
        [Column("idventa")]
        public int IdVenta { get; set; }
        
        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        [Column("vendedor")]
        public string Vendedor { get; set; } = null!;

        [Precision(18, 2)]
        [Column("total")]
        public decimal Total { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
