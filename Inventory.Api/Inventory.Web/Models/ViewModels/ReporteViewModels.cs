namespace Inventory.Web.Models.ViewModels
{
    public class ReporteVentaViewModel
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string VendedorId { get; set; } = null!;
        public string NombreVendedor { get; set; } = null!;
        public string EmailVendedor { get; set; } = null!;
        public List<ReporteVentaDetalleViewModel> Detalles { get; set; } = new List<ReporteVentaDetalleViewModel>();
    }

    public class ReporteVentaDetalleViewModel
    {
        public int IdDetalle { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Iva { get; set; }
        public decimal TotalDetalle { get; set; }
    }

    public class ReporteVentaResumenViewModel
    {
        public int TotalVentas { get; set; }
        public decimal MontoTotal { get; set; }
        public decimal IvaTotal { get; set; }
        public int TotalProductosVendidos { get; set; }
    }

    public class ReporteVentaCompletoViewModel
    {
        public List<ReporteVentaViewModel> Ventas { get; set; } = new List<ReporteVentaViewModel>();
        public ReporteVentaResumenViewModel Resumen { get; set; } = new ReporteVentaResumenViewModel();
    }

    public class VendedorViewModel
    {
        public string Id { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}

