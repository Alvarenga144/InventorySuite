namespace Inventory.Api.DTOs
{
    public class ProductoDto
    {
        public int IdPro { get; set; }
        public string Producto { get; set; } = null!;
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

