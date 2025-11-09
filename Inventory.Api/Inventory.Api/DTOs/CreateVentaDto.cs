using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.DTOs
{
    public class CreateVentaDto
    {
        [Required(ErrorMessage = "Los detalles de la venta son requeridos")]
        [MinLength(1, ErrorMessage = "Debe incluir al menos un producto")]
        public List<CreateDetalleVentaDto> Detalles { get; set; } = new List<CreateDetalleVentaDto>();
    }
}

