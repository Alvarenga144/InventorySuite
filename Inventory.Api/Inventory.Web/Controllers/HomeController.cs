using Inventory.Web.Filters;
using Inventory.Web.Helpers;
using Inventory.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;


namespace Inventory.Web.Controllers
{
    [AuthorizeFilter]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Obtener datos del usuario desde la sesión
            var userData = SessionHelper.GetUserData<UserSessionData>(HttpContext.Session);
            ViewBag.UserName = userData != null ? $"{userData.Nombre} {userData.Apellido}" : "Usuario";
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
