using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetLibrairie.Data;
using ProjetLibrairie.Helpers;
using ProjetLibrairie.Models;
using ProjetLibrairie.ViewModels;

namespace ProjetLibrairie.Controllers
{
    public class AdherentsController : Controller
    {
        private readonly BiblioDbContext _db;
        public AdherentsController(BiblioDbContext db) => _db = db;

        [RequireRole(AppRole.Admin)]
        public async Task<IActionResult> Index()
        {
            return View(await _db.Adherents.OrderBy(a => a.Nom).ThenBy(a => a.Prenom).ToListAsync());
        }


        [RequireRole(AppRole.Admin, AppRole.Adherent)]
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            // Si c'est un adhérent et qu'on n'a pas passé d'id => profil courant
            if (id == null && AuthSession.EstAdherent(HttpContext))
            {
                id = AuthSession.AdherentId(HttpContext);
            }

            if (id == null)
            {
                TempData["Error"] = "Adhérent introuvable.";
                return RedirectToAction(nameof(Index));
            }

            var adherent = await _db.Adherents
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id.Value);

            if (adherent == null)
            {
                TempData["Error"] = "Adhérent introuvable.";
                return RedirectToAction(nameof(Index));
            }

            return View(adherent);
        }

        [RequireRole(AppRole.Adherent)]
        [HttpGet]
        public async Task<IActionResult> Modifier(int id)
        {
            int currentId = AuthSession.AdherentId(HttpContext)!.Value;

            // Interdire de modifier quelqu'un d'autre
            if (id != currentId)
            {
                TempData["Error"] = "Accès refusé.";
                return RedirectToAction(nameof(Modifier), new { id = currentId });
            }

            var a = await _db.Adherents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == currentId);
            if (a == null) return NotFound();

            var vm = new ProfilViewModel
            {
                Id = a.Id,
                Nom = a.Nom,
                Prenom = a.Prenom,
                Email = a.Email,
                Login = a.Login
            };

            return View(vm);
        }

        [RequireRole(AppRole.Adherent)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modifier(ProfilViewModel vm)
        {
            int currentId = AuthSession.AdherentId(HttpContext)!.Value;

            if (vm.Id != currentId)
            {
                TempData["Error"] = "Accès refusé.";
                return RedirectToAction(nameof(Modifier), new { id = currentId });
            }

            if (!ModelState.IsValid) return View(vm);

            // unicité email/login (hors lui-même)
            bool emailExiste = await _db.Adherents.AnyAsync(a => a.Email == vm.Email && a.Id != currentId);
            if (emailExiste)
            {
                ModelState.AddModelError(nameof(vm.Email), "Cet email existe déjà.");
                TempData["Error"] = "Modification impossible : email déjà utilisé.";
                return View(vm);
            }

            bool loginExiste = await _db.Adherents.AnyAsync(a => a.Login == vm.Login && a.Id != currentId);
            if (loginExiste)
            {
                ModelState.AddModelError(nameof(vm.Login), "Ce login existe déjà.");
                TempData["Error"] = "Modification impossible : login déjà utilisé.";
                return View(vm);
            }

            var a = await _db.Adherents.FirstOrDefaultAsync(x => x.Id == currentId);
            if (a == null) return NotFound();

            a.Nom = vm.Nom.Trim();
            a.Prenom = vm.Prenom.Trim();
            a.Email = vm.Email.Trim();
            a.Login = vm.Login.Trim();

            if (!string.IsNullOrWhiteSpace(vm.NouveauMotDePasse))
            {
                a.MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(vm.NouveauMotDePasse);
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Profil mis à jour.";
            return RedirectToAction(nameof(Details), new { id = currentId });
        }

        [RequireRole(AppRole.Admin)]
        [HttpGet]
        public async Task<IActionResult> Bannir(int id)
        {
            var adherent = await _db.Adherents
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (adherent == null)
            {
                TempData["Error"] = "Adhérent introuvable.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.DateFin = DateTime.Now.AddMonths(1);
            return View(adherent);
        }

        [RequireRole(AppRole.Admin)]
        [HttpPost, ActionName("Bannir")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BannirConfirme(int id)
        {
            var adherent = await _db.Adherents.FirstOrDefaultAsync(a => a.Id == id);
            if (adherent == null)
            {
                TempData["Error"] = "Adhérent introuvable.";
                return RedirectToAction(nameof(Index));
            }

            adherent.DateFinBannissement = DateTime.Now.AddMonths(1);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Adhérent banni jusqu'au {adherent.DateFinBannissement:yyyy-MM-dd}.";
            return RedirectToAction(nameof(Index));
        }

        [RequireRole(AppRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Debannir(int id)
        {
            var adherent = await _db.Adherents.FirstOrDefaultAsync(a => a.Id == id);
            if (adherent == null)
            {
                TempData["Error"] = "Adhérent introuvable.";
                return RedirectToAction(nameof(Index));
            }

            adherent.DateFinBannissement = null;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Adhérent débanni.";
            return RedirectToAction(nameof(Index));
        }
    }
}
