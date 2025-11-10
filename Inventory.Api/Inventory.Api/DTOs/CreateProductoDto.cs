using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.DTOs
{
    public class CreateProductoDto
    {
        [Required(ErrorMessage = "El nombre del producto es requerido")]
        [MaxLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        public string Producto { get; set; } = null!;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Precio { get; set; }
    }
}

