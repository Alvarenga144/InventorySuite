namespace Inventory.Api.DTOs
{
    public class VentaDto
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string VendedorId { get; set; } = null!;
        public string VendedorNombre { get; set; } = null!;
        public decimal Total { get; set; }
        public List<DetalleVentaDto> Detalles { get; set; } = new List<DetalleVentaDto>();
    }
}

