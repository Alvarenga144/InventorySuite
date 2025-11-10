using System.ComponentModel.DataAnnotations;

namespace Inventory.Web.Models.ViewModels
{
    public class VentaViewModel
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string VendedorId { get; set; } = null!;
        public string VendedorNombre { get; set; } = null!;
        public decimal Total { get; set; }
        public List<DetalleVentaViewModel> Detalles { get; set; } = new List<DetalleVentaViewModel>();
    }

    public class DetalleVentaViewModel
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

    public class CreateVentaViewModel
    {
        [Required(ErrorMessage = "Debe agregar al menos un producto")]
        [MinLength(1, ErrorMessage = "Debe agregar al menos un producto")]
        public List<CreateDetalleVentaViewModel> Detalles { get; set; } = new List<CreateDetalleVentaViewModel>();
    }

    public class CreateDetalleVentaViewModel
    {
        [Required(ErrorMessage = "El ID del producto es requerido")]
        public int IdPro { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }
    }
}

