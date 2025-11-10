using System.ComponentModel.DataAnnotations;

namespace Inventory.Web.Models.ViewModels
{
    public class ProductoViewModel
    {
        public int IdPro { get; set; }
        public string Producto { get; set; } = null!;
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateProductoViewModel
    {
        [Required(ErrorMessage = "El nombre del producto es requerido")]
        [MaxLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        [Display(Name = "Producto")]
        public string Producto { get; set; } = null!;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }
    }

    public class UpdateProductoViewModel
    {
        [Required(ErrorMessage = "El nombre del producto es requerido")]
        [MaxLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        [Display(Name = "Producto")]
        public string Producto { get; set; } = null!;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }
    }
}

