using Dapper;
using Inventory.Api.Data;
using Inventory.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public ReportesController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Obtiene el reporte de ventas con filtro opcional por vendedor
        /// </summary>
        /// <param name="vendedorId">ID del vendedor (opcional). Si es null, trae todas las ventas</param>
        /// <returns>Reporte completo de ventas con detalles y resumen</returns>
        [HttpGet("ventas")]
        public async Task<ActionResult<ReporteVentaCompletoDto>> GetReporteVentas([FromQuery] string? vendedorId = null)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("Default");

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Ejecutar el stored procedure con múltiples result sets
                    using (var multi = await connection.QueryMultipleAsync(
                        "sp_ReporteVentas",
                        new { VendedorId = vendedorId },
                        commandType: System.Data.CommandType.StoredProcedure))
                    {
                        // Primer result set: Ventas con detalles (estructura plana)
                        var ventasPlanas = (await multi.ReadAsync<VentaPlanaTemporal>()).ToList();

                        // Segundo result set: Resumen
                        var resumen = await multi.ReadSingleOrDefaultAsync<ReporteVentaResumenDto>();

                        // Agrupar los datos planos en estructura jerárquica
                        var ventasAgrupadas = ventasPlanas
                            .GroupBy(v => new
                            {
                                v.IdVenta,
                                v.Fecha,
                                v.Total,
                                v.VendedorId,
                                v.NombreVendedor,
                                v.EmailVendedor
                            })
                            .Select(g => new ReporteVentaDto
                            {
                                IdVenta = g.Key.IdVenta,
                                Fecha = g.Key.Fecha,
                                Total = g.Key.Total,
                                VendedorId = g.Key.VendedorId,
                                NombreVendedor = g.Key.NombreVendedor,
                                EmailVendedor = g.Key.EmailVendedor,
                                Detalles = g.Select(d => new ReporteVentaDetalleDto
                                {
                                    IdDetalle = d.IdDetalle,
                                    IdProducto = d.IdProducto,
                                    NombreProducto = d.NombreProducto,
                                    Cantidad = d.Cantidad,
                                    PrecioUnitario = d.PrecioUnitario,
                                    Iva = d.Iva,
                                    TotalDetalle = d.TotalDetalle
                                }).ToList()
                            })
                            .ToList();

                        var resultado = new ReporteVentaCompletoDto
                        {
                            Ventas = ventasAgrupadas,
                            Resumen = resumen ?? new ReporteVentaResumenDto()
                        };

                        return Ok(resultado);
                    }
                }
            }
            catch (SqlException ex) when (ex.Message.Contains("El vendedor especificado no existe"))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al generar el reporte", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene la lista de vendedores disponibles para el filtro
        /// </summary>
        [HttpGet("vendedores")]
        public async Task<ActionResult<List<VendedorDto>>> GetVendedores()
        {
            try
            {
                var vendedores = await _context.Users
                    .Select(u => new VendedorDto
                    {
                        Id = u.Id,
                        NombreCompleto = u.Nombre + " " + u.Apellido,
                        Email = u.Email!
                    })
                    .OrderBy(v => v.NombreCompleto)
                    .ToListAsync();

                return Ok(vendedores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener vendedores", error = ex.Message });
            }
        }
    }

    // Clase temporal para mapear el resultado plano del SP
    internal class VentaPlanaTemporal
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string VendedorId { get; set; } = null!;
        public string NombreVendedor { get; set; } = null!;
        public string EmailVendedor { get; set; } = null!;
        public int IdDetalle { get; set; }
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; } = null!;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Iva { get; set; }
        public decimal TotalDetalle { get; set; }
    }

    // DTO para lista de vendedores
    public class VendedorDto
    {
        public string Id { get; set; } = null!;
        public string NombreCompleto { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}

