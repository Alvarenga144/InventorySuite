using Inventory.Web.Helpers;
using Inventory.Web.Models;
using Inventory.Web.Models.ViewModels;
using Inventory.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;

        public AccountController(IApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// GET: Mostrar formulario de login
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            // Si ya está autenticado, redirigir al Home
            if (SessionHelper.IsAuthenticated(HttpContext.Session))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        /// <summary>
        /// POST: Procesar login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiService.LoginAsync(model);

                if (response.Success && response.Data != null)
                {
                    // Guardar token en sesión
                    SessionHelper.SetToken(HttpContext.Session, response.Data.Token);

                    // Guardar datos del usuario en sesión
                    SessionHelper.SetUserData(HttpContext.Session, new UserSessionData
                    {
                        UserId = response.Data.UserId,
                        Email = response.Data.Email,
                        Nombre = response.Data.Nombre,
                        Apellido = response.Data.Apellido
                    });

                    TempData["SuccessMessage"] = $"¡Bienvenido {response.Data.Nombre}!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Credenciales inválidas");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al conectar con el servidor: {ex.Message}");
                return View(model);
            }
        }

        /// <summary>
        /// GET: Mostrar formulario de registro
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            // Si ya está autenticado, redirigir al Home
            if (SessionHelper.IsAuthenticated(HttpContext.Session))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        /// <summary>
        /// POST: Procesar registro
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiService.RegisterAsync(model);

                if (response.Success && response.Data != null)
                {
                    // Guardar token en sesión
                    SessionHelper.SetToken(HttpContext.Session, response.Data.Token);

                    // Guardar datos del usuario en sesión
                    SessionHelper.SetUserData(HttpContext.Session, new UserSessionData
                    {
                        UserId = response.Data.UserId,
                        Email = response.Data.Email,
                        Nombre = response.Data.Nombre,
                        Apellido = response.Data.Apellido
                    });

                    TempData["SuccessMessage"] = $"¡Registro exitoso! Bienvenido {response.Data.Nombre}!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Error al registrar usuario");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al conectar con el servidor: {ex.Message}");
                return View(model);
            }
        }

        /// <summary>
        /// POST: Cerrar sesión
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            SessionHelper.ClearSession(HttpContext.Session);
            TempData["InfoMessage"] = "Sesión cerrada exitosamente";
            return RedirectToAction("Login");
        }
    }
}

