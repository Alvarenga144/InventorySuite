namespace Inventory.Api.DTOs
{
    public class DetalleVentaDto
    {
        public int IdDet { get; set; }
        public DateTime Fecha { get; set; }
        public int IdPro { get; set; }
        public string ProductoNombre { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
    }
}

