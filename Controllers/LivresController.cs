using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjetLibrairie.Data;
using ProjetLibrairie.Helpers;
using ProjetLibrairie.Models;

namespace ProjetLibrairie.Controllers
{
    public class LivresController : Controller
    {
        private readonly BiblioDbContext _db;
        public LivresController(BiblioDbContext db) => _db = db;

        [RequireRole(AppRole.Admin, AppRole.Adherent)]
        public async Task<IActionResult> Index()
        {
            var livres = await _db.Livres.Include(l => l.Categorie).Include(l => l.Ratings).OrderBy(l => l.Titre).ToListAsync();
            return View(livres);
        }

        [RequireRole(AppRole.Admin, AppRole.Adherent)]
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var livre = await _db.Livres
                .Include(l => l.Categorie)
                .Include(l => l.Commentaires)
                    .ThenInclude(c => c.Adherent)
                .Include(l => l.Ratings)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id.Value);

            if (livre == null)
            {
                TempData["Error"] = "Livre introuvable.";
                return RedirectToAction(nameof(Index));
            }

            return View(livre);
        }

        [RequireRole(AppRole.Admin)]
        [HttpGet]
        public IActionResult Creer()
        {
            ViewBag.CategorieId = new SelectList(_db.Categories.OrderBy(c => c.Nom), "Id", "Nom");
            return View(new Livre());
        }

        [RequireRole(AppRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Creer(Livre livre)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.CategorieId = new SelectList(
                    await _db.Categories.OrderBy(c => c.Nom).ToListAsync(),
                    "Id", "Nom", livre.CategorieId
                );
                return View(livre);
            }

            livre.Disponible = true;
            _db.Livres.Add(livre);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Livre ajouté avec succès.";
            return RedirectToAction(nameof(Index));
        }

        [RequireRole(AppRole.Admin)]
        [HttpGet]
        public async Task<IActionResult> Modifier(int id)
        {

            var livre = await _db.Livres.FindAsync(id);
            if (livre == null)
            {
                TempData["Error"] = "Livre introuvable.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategorieId = new SelectList(
                await _db.Categories.OrderBy(c => c.Nom).ToListAsync(),
                "Id", "Nom", livre.CategorieId
            );

            return View(livre);
        }

        [RequireRole(AppRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modifier(Livre livre)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.CategorieId = new SelectList(
                    await _db.Categories.OrderBy(c => c.Nom).ToListAsync(),
                    "Id", "Nom", livre.CategorieId
                );
                return View(livre);
            }

            _db.Update(livre);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Livre modifié avec succès.";
            return RedirectToAction(nameof(Index));
        }

        [RequireRole(AppRole.Admin)]
        [HttpGet]
        public async Task<IActionResult> Supprimer(int id)
        {

            var livre = await _db.Livres.Include(l => l.Categorie).FirstOrDefaultAsync(l => l.Id == id);
            if (livre == null)
            {
                TempData["Error"] = "Livre introuvable.";
                return RedirectToAction(nameof(Index));
            }
            return View(livre);
        }

        [RequireRole(AppRole.Admin)]
        [HttpPost, ActionName("Supprimer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupprimerConfirme(int id)
        {

            var empruntActif = await _db.Emprunts.AnyAsync(e => e.LivreId == id && e.DateRetour == null);
            var livre = await _db.Livres.Include(l => l.Categorie).FirstOrDefaultAsync(l => l.Id == id);

            if (livre == null)
            {
                TempData["Error"] = "Livre introuvable.";
                return RedirectToAction(nameof(Index));
            }

            if (empruntActif)
            {
                TempData["Error"] = "Impossible de supprimer : livre actuellement emprunté.";
                return View(livre);
            }

            _db.Livres.Remove(livre);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Livre supprimé avec succès.";
            return RedirectToAction(nameof(Index));
        }
    }
}
