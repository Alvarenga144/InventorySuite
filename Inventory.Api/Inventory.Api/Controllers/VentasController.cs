using Inventory.Api.Data;
using Inventory.Api.DTOs;
using Inventory.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Inventory.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public VentasController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Obtener todas las ventas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VentaDto>>> GetVentas()
        {
            try
            {
                var ventas = await _context.Ventas
                    .Include(v => v.Vendedor)
                    .OrderByDescending(v => v.Fecha)
                    .Select(v => new VentaDto
                    {
                        IdVenta = v.IdVenta,
                        Fecha = v.Fecha,
                        VendedorId = v.VendedorId,
                        VendedorNombre = v.Vendedor.Nombre + " " + v.Vendedor.Apellido,
                        Total = v.Total,
                        Detalles = new List<DetalleVentaDto>() // Lista vacía, se llena en GetVenta por ID
                    })
                    .ToListAsync();

                return Ok(ventas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener ventas", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener una venta por ID con sus detalles
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<VentaDto>> GetVenta(int id)
        {
            try
            {
                var venta = await _context.Ventas
                    .Include(v => v.Vendedor)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.IdVenta == id);

                if (venta == null)
                {
                    return NotFound(new { message = $"Venta con ID {id} no encontrada" });
                }

                var ventaDto = new VentaDto
                {
                    IdVenta = venta.IdVenta,
                    Fecha = venta.Fecha,
                    VendedorId = venta.VendedorId,
                    VendedorNombre = venta.Vendedor.Nombre + " " + venta.Vendedor.Apellido,
                    Total = venta.Total,
                    Detalles = venta.Detalles.Select(d => new DetalleVentaDto
                    {
                        IdDet = d.IdDet,
                        Fecha = d.Fecha,
                        IdPro = d.IdPro,
                        ProductoNombre = d.Producto.ProductoNombre,
                        Cantidad = d.Cantidad,
                        Precio = d.Precio,
                        Iva = d.Iva,
                        Total = d.Total
                    }).ToList()
                };

                return Ok(ventaDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la venta", error = ex.Message });
            }
        }

        /// <summary>
        /// Crear una nueva venta
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<VentaDto>> CreateVenta(CreateVentaDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                // IVA
                var ivaPorcentaje = _configuration.GetValue<decimal>("AppSettings:IvaPorcentaje") / 100;

                // Validar que todos los productos existan y estén activos
                var productosIds = dto.Detalles.Select(d => d.IdPro).Distinct().ToList();
                var productos = await _context.Productos
                    .Where(p => productosIds.Contains(p.IdPro) && p.Activo)
                    .ToListAsync();

                if (productos.Count != productosIds.Count)
                {
                    var productosNoEncontrados = productosIds.Except(productos.Select(p => p.IdPro)).ToList();
                    return BadRequest(new { message = "Algunos productos no existen o no están activos", productosNoEncontrados });
                }

                var fechaVenta = DateTime.UtcNow;

                // Crear la venta (encabezado)
                var venta = new Venta
                {
                    Fecha = fechaVenta,
                    VendedorId = userId,
                    Total = 0, // Se calculará después
                    CreatedAt = fechaVenta
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                decimal totalVenta = 0;

                // Crear los detalles de venta
                foreach (var detalleDto in dto.Detalles)
                {
                    var producto = productos.First(p => p.IdPro == detalleDto.IdPro);

                    // Calcular valores
                    var precioUnitario = producto.Precio;
                    var subtotal = precioUnitario * detalleDto.Cantidad;
                    var iva = subtotal * ivaPorcentaje;
                    var totalLinea = subtotal + iva;

                    var detalle = new DetalleVenta
                    {
                        Fecha = fechaVenta,
                        IdVenta = venta.IdVenta,
                        IdPro = detalleDto.IdPro,
                        Cantidad = detalleDto.Cantidad,
                        Precio = precioUnitario,
                        Iva = iva,
                        Total = totalLinea
                    };

                    _context.DetalleVentas.Add(detalle);
                    totalVenta += totalLinea;
                }

                // Actualizar el total de la venta
                venta.Total = totalVenta;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Obtener la venta completa para la respuesta
                var ventaCreada = await _context.Ventas
                    .Include(v => v.Vendedor)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstAsync(v => v.IdVenta == venta.IdVenta);

                var ventaDto = new VentaDto
                {
                    IdVenta = ventaCreada.IdVenta,
                    Fecha = ventaCreada.Fecha,
                    VendedorId = ventaCreada.VendedorId,
                    VendedorNombre = ventaCreada.Vendedor.Nombre + " " + ventaCreada.Vendedor.Apellido,
                    Total = ventaCreada.Total,
                    Detalles = ventaCreada.Detalles.Select(d => new DetalleVentaDto
                    {
                        IdDet = d.IdDet,
                        Fecha = d.Fecha,
                        IdPro = d.IdPro,
                        ProductoNombre = d.Producto.ProductoNombre,
                        Cantidad = d.Cantidad,
                        Precio = d.Precio,
                        Iva = d.Iva,
                        Total = d.Total
                    }).ToList()
                };

                return CreatedAtAction(nameof(GetVenta), new { id = venta.IdVenta }, ventaDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Error al crear la venta", error = ex.Message });
            }
        }
    }
}

