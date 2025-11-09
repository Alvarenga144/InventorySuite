using Inventory.Web.Filters;
using Inventory.Web.Helpers;
using Inventory.Web.Models;
using Inventory.Web.Models.ViewModels;
using Inventory.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers
{
    [AuthorizeFilter]
    public class VentasController : Controller
    {
        private readonly IApiService _apiService;

        public VentasController(IApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// GET: Listar todas las ventas
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = SessionHelper.GetToken(HttpContext.Session);
                var response = await _apiService.GetVentasAsync(token);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? "Error al obtener ventas";
                    return View(new List<VentaViewModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión: {ex.Message}";
                return View(new List<VentaViewModel>());
            }
        }

        /// <summary>
        /// GET: Ver detalle de una venta
        /// </summary>
        public async Task<IActionResult> Detalle(int id)
        {
            try
            {
                var token = SessionHelper.GetToken(HttpContext.Session);
                var response = await _apiService.GetVentaByIdAsync(id, token);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? "Venta no encontrada";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// GET: Mostrar formulario de nueva venta
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST: Crear una nueva venta
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVentaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validar que haya al menos un producto
            if (model.Detalles == null || !model.Detalles.Any())
            {
                ModelState.AddModelError(string.Empty, "Debe agregar al menos un producto a la venta");
                return View(model);
            }

            try
            {
                var token = SessionHelper.GetToken(HttpContext.Session);
                var response = await _apiService.CreateVentaAsync(model, token);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Venta registrada exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Error al crear la venta");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error de conexión: {ex.Message}");
                return View(model);
            }
        }

        /// <summary>
        /// GET: Obtener producto por ID (usado por AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProducto(int id)
        {
            try
            {
                var token = SessionHelper.GetToken(HttpContext.Session);
                var response = await _apiService.GetProductoByIdAsync(id, token);

                if (response.Success && response.Data != null)
                {
                    return Json(new { success = true, data = response.Data });
                }
                else
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
    }
}

