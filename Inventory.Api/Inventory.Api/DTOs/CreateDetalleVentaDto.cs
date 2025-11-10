using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.DTOs
{
    public class CreateDetalleVentaDto
    {
        [Required(ErrorMessage = "El ID del producto es requerido")]
        public int IdPro { get; set; }

        [Required(ErrorMessage = "La cantidad es requerida")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }
    }
}

