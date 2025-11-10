namespace Inventory.Api.DTOs
{
    public class ReporteVentaDto
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string VendedorId { get; set; } = null!;
        public string NombreVendedor { get; set; } = null!;
        public string EmailVendedor { get; set; } = null!;
        public List<ReporteVentaDetalleDto> Detalles { get; set; } = new List<ReporteVentaDetalleDto>();
    }

    public class ReporteVentaDetalleDto
    {
        public int IdDetalle { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Iva { get; set; }
        public decimal TotalDetalle { get; set; }
    }

    public class ReporteVentaResumenDto
    {
        public int TotalVentas { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal IvaTotal { get; set; }
        public int TotalProductosVendidos { get; set; }
    }

    public class ReporteVentaCompletoDto
    {
        public List<ReporteVentaDto> Ventas { get; set; } = new List<ReporteVentaDto>();
        public ReporteVentaResumenDto Resumen { get; set; } = new ReporteVentaResumenDto();
    }
}

