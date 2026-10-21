using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetLibrairie.Data;
using ProjetLibrairie.Helpers;
using ProjetLibrairie.Models;

namespace ProjetLibrairie.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly BiblioDbContext _db;
        public CategoriesController(BiblioDbContext db) => _db = db;

        [RequireRole(AppRole.Admin)]
        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Nom)
                .ToListAsync();

            return View(categories);
        }

        [RequireRole(AppRole.Admin)]
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var categorie = await _db.Categories
                .AsNoTracking()
                .Include(c => c.Livres)
                .FirstOrDefaultAsync(c => c.Id == id.Value);

            if (categorie == null) return NotFound();

            // optionnel : trier les livres
            categorie.Livres = categorie.Livres.OrderBy(l => l.Titre).ToList();

            return View(categorie);
        }

        [RequireRole(AppRole.Admin)]
        [HttpGet]
        public IActionResult Creer() => View(new Categorie());

        [RequireRole(AppRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Creer(Categorie categorie)
        {
            if (!ModelState.IsValid) return View(categorie);

            bool existe = await _db.Categories.AnyAsync(c => c.Nom == categorie.Nom);
            if (existe)
            {
                ModelState.AddModelError(nameof(Categorie.Nom), "Cette catégorie existe déjà.");
                return View(categorie);
            }

            _db.Categories.Add(categorie);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Catégorie créée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        [RequireRole(AppRole.Admin)]
        [HttpGet]
        public async Task<IActionResult> Modifier(int? id)
        {
            if (id == null) return NotFound();

            var categorie = await _db.Categories.FindAsync(id.Value);
            if (categorie == null) return NotFound();

            return View(categorie);
        }

        [RequireRole(AppRole.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modifier(Categorie categorie)
        {
            if (!ModelState.IsValid) return View(categorie);

            var existante = await _db.Categories.FindAsync(categorie.Id);
            if (existante == null) return NotFound();

            bool doublon = await _db.Categories.AnyAsync(c => c.Id != categorie.Id && c.Nom == categorie.Nom);
            if (doublon)
            {
                ModelState.AddModelError(nameof(Categorie.Nom), "Une autre catégorie porte déjà ce nom.");
                return View(categorie);
            }

            existante.Nom = categorie.Nom;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Catégorie modifiée avec succès.";
            return RedirectToAction(nameof(Index));
        }

        [RequireRole(AppRole.Admin)]
        [HttpGet]
        public async Task<IActionResult> Supprimer(int? id)
        {
            if (id == null) return NotFound();

            var categorie = await _db.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id.Value);

            if (categorie == null) return NotFound();
            return View(categorie);
        }

        [RequireRole(AppRole.Admin)]
        [HttpPost, ActionName("Supprimer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupprimerConfirme(int id)
        {
            var categorie = await _db.Categories
                .Include(c => c.Livres)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categorie == null) return NotFound();

            if (categorie.Livres.Any())
            {
                TempData["Error"] = "Impossible de supprimer : des livres sont associés à cette catégorie.";
                return RedirectToAction(nameof(Supprimer), new { id });
            }

            _db.Categories.Remove(categorie);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Catégorie supprimée avec succès.";
            return RedirectToAction(nameof(Index));
        }
    }
}