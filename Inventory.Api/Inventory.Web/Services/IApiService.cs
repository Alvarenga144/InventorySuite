using Inventory.Web.Models.ViewModels;

namespace Inventory.Web.Services
{
    public interface IApiService
    {
        // Authentication
        Task<ApiResponse<AuthResponseViewModel>> RegisterAsync(RegisterViewModel model);
        Task<ApiResponse<AuthResponseViewModel>> LoginAsync(LoginViewModel model);

        // Productos
        Task<ApiResponse<List<ProductoViewModel>>> GetProductosAsync(string? token);
        Task<ApiResponse<ProductoViewModel>> GetProductoByIdAsync(int id, string? token);
        Task<ApiResponse<ProductoViewModel>> CreateProductoAsync(CreateProductoViewModel model, string? token);
        Task<ApiResponse<ProductoViewModel>> UpdateProductoAsync(int id, UpdateProductoViewModel model, string? token);
        Task<ApiResponse<object>> DeleteProductoAsync(int id, string? token);

        // Ventas
        Task<ApiResponse<List<VentaViewModel>>> GetVentasAsync(string? token);
        Task<ApiResponse<VentaViewModel>> GetVentaByIdAsync(int id, string? token);
        Task<ApiResponse<VentaViewModel>> CreateVentaAsync(CreateVentaViewModel model, string? token);

        // Reportes
        Task<ApiResponse<ReporteVentaCompletoViewModel>> GetReporteVentasAsync(string? vendedorId, string? token);
        Task<ApiResponse<List<VendedorViewModel>>> GetVendedoresAsync(string? token);
    }

    // Clase auxiliar para manejar respuestas de la API
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }
    }
}

