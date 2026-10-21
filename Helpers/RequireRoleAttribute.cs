using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace ProjetLibrairie.Helpers
{
    public enum AppRole
    {
        Admin,
        Adherent
    }

    public class RequireRoleAttribute : ActionFilterAttribute
    {
        private readonly AppRole[] _roles;

        public RequireRoleAttribute(params AppRole[] roles)
        {
            _roles = roles ?? new AppRole[0];
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var http = context.HttpContext;

            bool ok = _roles.Any(r => r switch
            {
                AppRole.Admin => AuthSession.EstAdmin(http),
                AppRole.Adherent => AuthSession.EstAdherent(http),
                _ => false
            });

            if (!ok)
            {
                // Ecrit dans TempData si on est bien dans un controller MVC (Views)
                if (context.Controller is Controller ctrl)
                {
                    ctrl.TempData["Error"] = "Accès refusé.";
                }

                // Retour à la page précédente (impression de "rester sur la même page")
                var referer = http.Request.Headers.Referer.ToString();
                if (!string.IsNullOrWhiteSpace(referer))
                {
                    context.Result = new RedirectResult(referer);
                }
                else
                {
                    // fallback si l'utilisateur tape l'URL directement
                    context.Result = new RedirectToActionResult("Index", "Accueil", null);
                }

                return;
            }

            base.OnActionExecuting(context);
        }
    }
}