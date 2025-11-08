using Inventory.Api.Data;
using Inventory.Api.DTOs;
using Inventory.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly DataContext _context;

        public ProductosController(DataContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtener todos los productos activos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductoDto>>> GetProductos()
        {
            try
            {
                var productos = await _context.Productos
                    .Where(p => p.Activo)
                    .OrderBy(p => p.ProductoNombre)
                    .Select(p => new ProductoDto
                    {
                        IdPro = p.IdPro,
                        Producto = p.ProductoNombre,
                        Precio = p.Precio,
                        Activo = p.Activo,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener productos", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener un producto por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductoDto>> GetProducto(int id)
        {
            try
            {
                var producto = await _context.Productos
                    .Where(p => p.IdPro == id)
                    .Select(p => new ProductoDto
                    {
                        IdPro = p.IdPro,
                        Producto = p.ProductoNombre,
                        Precio = p.Precio,
                        Activo = p.Activo,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (producto == null)
                {
                    return NotFound(new { message = $"Producto con ID {id} no encontrado" });
                }

                return Ok(producto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el producto", error = ex.Message });
            }
        }

        /// <summary>
        /// Crear un nuevo producto
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductoDto>> CreateProducto(CreateProductoDto dto)
        {
            try
            {
                var producto = new Producto
                {
                    ProductoNombre = dto.Producto,
                    Precio = dto.Precio,
                    Activo = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                var productoDto = new ProductoDto
                {
                    IdPro = producto.IdPro,
                    Producto = producto.ProductoNombre,
                    Precio = producto.Precio,
                    Activo = producto.Activo,
                    CreatedAt = producto.CreatedAt,
                    UpdatedAt = producto.UpdatedAt
                };

                return CreatedAtAction(nameof(GetProducto), new { id = producto.IdPro }, productoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el producto", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar un producto existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductoDto>> UpdateProducto(int id, UpdateProductoDto dto)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);

                if (producto == null)
                {
                    return NotFound(new { message = $"Producto con ID {id} no encontrado" });
                }

                // Actualizar campos
                producto.ProductoNombre = dto.Producto;
                producto.Precio = dto.Precio;
                producto.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var productoDto = new ProductoDto
                {
                    IdPro = producto.IdPro,
                    Producto = producto.ProductoNombre,
                    Precio = producto.Precio,
                    Activo = producto.Activo,
                    CreatedAt = producto.CreatedAt,
                    UpdatedAt = producto.UpdatedAt
                };

                return Ok(productoDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el producto", error = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar un producto (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);

                if (producto == null)
                {
                    return NotFound(new { message = $"Producto con ID {id} no encontrado" });
                }

                // Verificar si el producto tiene ventas asociadas
                var tieneVentas = await _context.DetalleVentas
                    .AnyAsync(dv => dv.IdPro == id);

                if (tieneVentas)
                {
                    return BadRequest(new { message = "No se puede eliminar el producto porque tiene ventas asociadas" });
                }

                // Soft delete: marcar como inactivo
                producto.Activo = false;
                producto.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Producto eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el producto", error = ex.Message });
            }
        }
    }
}
