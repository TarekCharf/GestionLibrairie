using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetLibrairie.Data;
using ProjetLibrairie.Helpers;
using ProjetLibrairie.Models;
using Rotativa.AspNetCore;

namespace ProjetLibrairie.Controllers
{
    public class EmpruntsController : Controller
    {
        private readonly BiblioDbContext _db;
        public EmpruntsController(BiblioDbContext db) => _db = db;

        [RequireRole(AppRole.Admin)]
        public async Task<IActionResult> Index()
        {
            var emprunts = await _db.Emprunts
                .Include(e => e.Livre)
                .Include(e => e.Adherent)
                .OrderByDescending(e => e.DateEmprunt)
                .ToListAsync();

            return View(emprunts);
        }

        [RequireRole(AppRole.Admin, AppRole.Adherent)]
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var emprunt = await _db.Emprunts
                .AsNoTracking()
                .Include(e => e.Livre)
                .Include(e => e.Adherent)
                .FirstOrDefaultAsync(e => e.Id == id.Value);

            if (emprunt == null)
            {
                TempData["Error"] = "Emprunt introuvable.";
                return RedirectToAction(nameof(Index));
            }
            return View(emprunt);
        }

        [RequireRole(AppRole.Adherent)]
        [HttpGet]
        public async Task<IActionResult> Creer(int livreId)
        {
            int adherentId = AuthSession.AdherentId(HttpContext)!.Value;

            var livre = await _db.Livres
                .AsNoTracking()
                .Include(l => l.Categorie)
                .FirstOrDefaultAsync(l => l.Id == livreId);

            if (livre == null) return NotFound();

            if (!livre.Disponible)
            {
                TempData["Error"] = "Ce livre n'est plus disponible.";
                return RedirectToAction(nameof(LivresDisponibles));
            }

            var hasEmprunt = await _db.Emprunts
                .AnyAsync(e => e.AdherentId == adherentId && e.DateRetour == null);

            if (hasEmprunt)
            {
                TempData["Error"] = "Vous avez déjà un emprunt en cours. Veuillez le retourner avant d'en emprunter un autre.";
                return RedirectToAction(nameof(MesEmprunts));
            }

            var now = DateTime.Now;

            ViewBag.Livre = livre;
            ViewBag.DateLimite = now.AddDays(14);

            return View(new Emprunt
            {
                LivreId = livreId,
                AdherentId = adherentId,
                DateEmprunt = now,
                DateRetour = null
            });
        }

        [RequireRole(AppRole.Adherent)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Creer(Emprunt emprunt)
        {
            int adherentId = AuthSession.AdherentId(HttpContext)!.Value;

            // On force les champs côté serveur
            var now = DateTime.Now;
            emprunt.AdherentId = adherentId;
            emprunt.DateEmprunt = now;
            emprunt.DateRetour = null;

            var livre = await _db.Livres.Include(l => l.Categorie).FirstOrDefaultAsync(l => l.Id == emprunt.LivreId);
            if (livre == null) return NotFound();

            if (!livre.Disponible)
            {
                TempData["Error"] = "Ce livre n'est plus disponible.";
                return RedirectToAction(nameof(LivresDisponibles));
            }

            _db.Emprunts.Add(emprunt);
            livre.Disponible = false;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Emprunt enregistré. Date limite : {now.AddDays(14):yyyy-MM-dd}.";
            return RedirectToAction(nameof(AttestationPdf), new { id = emprunt.Id });
        }

        [RequireRole(AppRole.Adherent)]
        public async Task<IActionResult> AttestationPdf(int id)
        {
            var emprunt = await _db.Emprunts
                .Include(e => e.Adherent)
                .Include(e => e.Livre)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprunt == null)
                return NotFound();

            return new ViewAsPdf(
                "AttestationPdf",
                emprunt
            )
            {
                FileName = $"Emprunt_{id}.pdf"
            };
        }

        [RequireRole(AppRole.Admin)]
        [HttpGet]
        public async Task<IActionResult> Retour(int id)
        {

            var emprunt = await _db.Emprunts
                .Include(e => e.Livre)
                .Include(e => e.Adherent)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (emprunt == null)
            {
                TempData["Error"] = "Emprunt introuvable.";
                return RedirectToAction(nameof(Index));
            }
            return View(emprunt);
        }

        [RequireRole(AppRole.Admin)]
        [HttpPost, ActionName("Retour")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RetourConfirme(int id)
        {

            var emprunt = await _db.Emprunts.Include(e => e.Livre).FirstOrDefaultAsync(e => e.Id == id);
            if (emprunt == null)
            {                 
                TempData["Error"] = "Emprunt introuvable.";
                return RedirectToAction(nameof(Index));
            }

            if (emprunt.DateRetour == null)
            {
                emprunt.DateRetour = DateTime.Now;
                if (emprunt.Livre != null) emprunt.Livre.Disponible = true;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Retour enregistré.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Adhérent: ses emprunts
        [RequireRole(AppRole.Adherent)]
        public async Task<IActionResult> MesEmprunts()
        {
            if (!AuthSession.EstAdherent(HttpContext))
            {
                TempData["Error"] = "Veuillez vous connecter.";
                return RedirectToAction("Connexion", "Compte");
            }

            int adherentId = AuthSession.AdherentId(HttpContext)!.Value;

            var emprunts = await _db.Emprunts
                .Include(e => e.Livre)
                .Where(e => e.AdherentId == adherentId)
                .OrderByDescending(e => e.DateEmprunt)
                .ToListAsync();

            return View(emprunts);
        }

        [RequireRole(AppRole.Admin, AppRole.Adherent)]
        public async Task<IActionResult> LivresDisponibles()
        {
            var livres = await _db.Livres.Include(l => l.Categorie).Where(l => l.Disponible).OrderBy(l => l.Titre).ToListAsync();
            return View(livres);
        }

        [RequireRole(AppRole.Admin, AppRole.Adherent)]
        public async Task<IActionResult> LivresEmpruntes()
        {
            var livres = await _db.Livres.Include(l => l.Categorie).Where(l => !l.Disponible).OrderBy(l => l.Titre).ToListAsync();
            return View(livres);
        }

        [RequireRole(AppRole.Admin, AppRole.Adherent)]
        private async Task RemplirListes(int? livreId, int? adherentId)
        {
            ViewBag.LivreId = new SelectList(
                await _db.Livres.Where(l => l.Disponible).OrderBy(l => l.Titre).ToListAsync(),
                "Id", "Titre", livreId);

            ViewBag.AdherentId = new SelectList(
                await _db.Adherents.OrderBy(a => a.Nom).ThenBy(a => a.Prenom).ToListAsync(),
                "Id", "Email", adherentId);
        }
    }
}
