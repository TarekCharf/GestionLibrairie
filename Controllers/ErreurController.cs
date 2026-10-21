using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using ProjetLibrairie.Models;

namespace ProjetLibrairie.Controllers
{
    public class ErreurController : Controller
    {
        [Route("Erreur")]
        public IActionResult Index()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var vm = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                // Tu peux ajouter ces champs à ErrorViewModel si tu veux (voir plus bas)
            };

            ViewBag.ExceptionMessage = feature?.Error?.Message;
            ViewBag.Path = feature?.Path;

            return View("Error", vm);
        }

        // Optionnel: page pour erreurs HTTP (404, 403, etc.)
        [Route("Erreur/{statusCode:int}")]
        public IActionResult HttpStatusCode(int statusCode)
        {
            ViewBag.StatusCode = statusCode;
            return View("HttpError");
        }
    }
}
