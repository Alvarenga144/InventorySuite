using Inventory.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Inventory.Web.Filters
{
    /// <summary>
    /// Filtro para verificar que el usuario esté autenticado antes de acceder a un controller/action
    /// </summary>
    public class AuthorizeFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;

            // Verificar si el usuario está autenticado
            if (!SessionHelper.IsAuthenticated(session))
            {
                // Redirigir al login si no está autenticado
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}

