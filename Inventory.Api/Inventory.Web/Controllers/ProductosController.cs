using Inventory.Web.Filters;
using Inventory.Web.Helpers;
using Inventory.Web.Models.ViewModels;
using Inventory.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers
{
    [AuthorizeFilter]
    public class ProductosController : Controller
    {
        private readonly IApiService _apiService;

        public ProductosController(IApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// GET: Listar todos los productos
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = SessionHelper.GetToken(HttpContext.Session);
                var response = await _apiService.GetProductosAsync(token);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? "Error al obtener productos";
                    return View(new List<ProductoViewModel>());
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión: {ex.Message}";
                return View(new List<ProductoViewModel>());
            }
        }

        /// <summary>
        /// GET: Mostrar formulario de crear producto
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// POST: Crear un nuevo producto
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = SessionHelper.GetToken(HttpContext.Session);
                var response = await _apiService.CreateProductoAsync(model, token);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Producto creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Error al crear producto");
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
        /// GET: Mostrar formulario de editar producto
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var token = SessionHelper.GetToken(HttpContext.Session);
                var response = await _apiService.GetProductoByIdAsync(id, token);

                if (response.Success && response.Data != null)
                {
                    var model = new UpdateProductoViewModel
                    {
                        Producto = response.Data.Producto,
                        Precio = response.Data.Precio
                    };
                    return View(model);
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? "Producto no encontrado";
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
        /// POST: Actualizar un producto
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateProductoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var token = SessionHelper.GetToken(HttpContext.Session);
                var response = await _apiService.UpdateProductoAsync(id, model, token);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Producto actualizado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Error al actualizar producto");
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
        /// POST: Eliminar un producto (soft delete)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var token = SessionHelper.GetToken(HttpContext.Session);
                var response = await _apiService.DeleteProductoAsync(id, token);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Producto eliminado exitosamente";
                }
                else
                {
                    TempData["ErrorMessage"] = response.ErrorMessage ?? "Error al eliminar producto";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error de conexión: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

