using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using ASP_KN_P_212.Middleware;
using System.Security.Claims;

namespace ASP_KN_P_212.Controllers
{
    public class BackendController : ControllerBase, IActionFilter
    {
        protected bool isAuthenticated;
        protected bool isAdmin;

        [NonAction] public void OnActionExecuting(ActionExecutingContext context)
        {
            var identity = User.Identities
                .FirstOrDefault(i => i.AuthenticationType == nameof(AuthSessionMiddleware));

            identity ??= User.Identities
                .FirstOrDefault(i => i.AuthenticationType == nameof(AuthTokenMiddleware));

            this.isAuthenticated = identity != null;

            String? userRole = identity?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            this.isAdmin = "Admin".Equals(userRole);
        }

        [NonAction] public void OnActionExecuted(ActionExecutedContext context)
        { }
    }
}
