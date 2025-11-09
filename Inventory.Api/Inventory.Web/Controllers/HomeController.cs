using Inventory.Web.Filters;
using Inventory.Web.Helpers;
using Inventory.Web.Models;
using Inventory.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace Inventory.Web.Controllers
{
    [AuthorizeFilter]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IApiService _apiService;

        public HomeController(ILogger<HomeController> logger, IApiService apiService)
        {
            _logger = logger;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            // Obtener datos del usuario desde la sesión
            var userData = SessionHelper.GetUserData<UserSessionData>(HttpContext.Session);
            ViewBag.UserName = userData != null ? $"{userData.Nombre} {userData.Apellido}" : "Usuario";

            var token = SessionHelper.GetToken(HttpContext.Session);

            try
            {
                var productosResponse = await _apiService.GetProductosAsync(token);
                ViewBag.TotalProductos = productosResponse.Success && productosResponse.Data != null
                    ? productosResponse.Data.Count
                    : 0;
            }
            catch
            {
                ViewBag.TotalProductos = 0;
            }

            try
            {
                var ventasResponse = await _apiService.GetVentasAsync(token);
                var hoy = DateTime.Today;
                ViewBag.TotalVentasHoy = ventasResponse.Success && ventasResponse.Data != null
                    ? ventasResponse.Data.Count(v => v.Fecha.Date == hoy)
                    : 0;
            }
            catch
            {
                ViewBag.TotalVentasHoy = 0;
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
