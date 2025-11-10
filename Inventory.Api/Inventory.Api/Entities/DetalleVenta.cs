using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventory.Api.Entities
{
    public class DetalleVenta
    {
        [Key]
        [Column("idde")]
        public int IdDet { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        // FK a Venta
        [Column("idventa")]
        public int IdVenta { get; set; }

        public Venta Venta { get; set; } = null!;

        // FK a Producto
        [Column("idpro")]
        public int IdPro { get; set; }

        public Producto Producto { get; set; } = null!;

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Precision(18, 2)]
        [Column("precio")]
        public decimal Precio { get; set; }

        [Precision(18, 2)]
        [Column("iva")]
        public decimal Iva { get; set; }

        [Precision(18, 2)]
        [Column("total")]
        public decimal Total { get; set; }
    }
}
