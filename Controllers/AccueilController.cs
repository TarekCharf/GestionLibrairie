using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetLibrairie.Data;
using ProjetLibrairie.ViewModels;

namespace ProjetLibrairie.Controllers
{
    public class AccueilController : Controller
    {
        private readonly BiblioDbContext _db;
        public AccueilController(BiblioDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var vm = new StatistiquesViewModel
            {
                EmpruntsActifs = await _db.Emprunts.CountAsync(e => e.DateRetour == null),
                LivresDisponibles = await _db.Livres.CountAsync(l => l.Disponible),
                LivresEmpruntes = await _db.Livres.CountAsync(l => !l.Disponible),
                NombreAdherents = await _db.Adherents.CountAsync()
            };

            // Top 5 livres les plus empruntés
            vm.TopLivres = await _db.Emprunts
                .AsNoTracking()
                .GroupBy(e => e.LivreId)
                .Select(g => new { LivreId = g.Key, Nb = g.Count() })
                .OrderByDescending(x => x.Nb)
                .Take(5)
                .Join(_db.Livres.AsNoTracking(),
                      x => x.LivreId,
                      l => l.Id,
                      (x, l) => new ProjetLibrairie.ViewModels.TopLivreItem
                      {
                          LivreId = l.Id,
                          Titre = l.Titre,
                          Auteur = l.Auteur,
                          NbEmprunts = x.Nb
                      })
                .ToListAsync();

            // Top 5 adhérents qui empruntent le plus
            vm.TopAdherents = await _db.Emprunts
                .AsNoTracking()
                .GroupBy(e => e.AdherentId)
                .Select(g => new { AdherentId = g.Key, Nb = g.Count() })
                .OrderByDescending(x => x.Nb)
                .Take(5)
                .Join(_db.Adherents.AsNoTracking(),
                      x => x.AdherentId,
                      a => a.Id,
                      (x, a) => new ProjetLibrairie.ViewModels.TopAdherentItem
                      {
                          AdherentId = a.Id,
                          NomComplet = a.Nom + " " + a.Prenom,
                          Email = a.Email,
                          NbEmprunts = x.Nb
                      })
                .ToListAsync();

            vm.TopCategories = await _db.Categories
                .AsNoTracking()
                .Where(l => l.Id != null)
                .GroupBy(l => l.Nom)
                .Select(g => new ProjetLibrairie.ViewModels.TopCategorieItem
                {
                    Nom = g.Key,
                    NbEmprunts = g.Count()
                })
                .OrderByDescending(x => x.NbEmprunts)
                .Take(5)
                .ToListAsync();

            vm.TopLivresRatings = await _db.Ratings
                .AsNoTracking()
                .GroupBy(r => r.LivreId)
                .Select(g => new
                {
                    LivreId = g.Key,
                    Rating = g.Average(r => r.Valeur),
                    NbRatings = g.Count()
                })
                .Join(
                    _db.Livres,
                    r => r.LivreId,
                    l => l.Id,
                    (r, l) => new
                    {
                        l.Id,
                        l.Titre,
                        l.Auteur,
                        Rating = r.Rating,
                        NbRatings = r.NbRatings
                    }
                )
                .OrderByDescending(x => x.Rating)
                .ThenByDescending(x => x.NbRatings)
                .Take(5)
                .Select(x => new ProjetLibrairie.ViewModels.TopLivreRatingItem
                {
                    LivreId = x.Id,
                    Titre = x.Titre,
                    Auteur = x.Auteur,
                    NoteMoyenne = x.Rating,
                    NbRatings = x.NbRatings
                })
                .ToListAsync();

            return View(vm);
        }

        public async Task<IActionResult> Recherche(string? q)
        {
            var query = _db.Livres
                .Include(l => l.Categorie)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();

                query = query.Where(l =>
                    l.Titre.Contains(q) ||
                    l.Auteur.Contains(q) ||
                    (l.Categorie != null && l.Categorie.Nom.Contains(q))
                );
            }

            var result = await query
                .OrderBy(l => l.Titre)
                .ToListAsync();

            ViewBag.Q = q ?? "";
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> RecherchePartial(string? q)
        {
            var query = _db.Livres
                .Include(l => l.Categorie)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();

                query = query.Where(l =>
                    l.Titre.Contains(q) ||
                    l.Auteur.Contains(q) ||
                    (l.Categorie != null && l.Categorie.Nom.Contains(q))
                );
            }

            var result = await query
                .OrderBy(l => l.Titre)
                .ToListAsync();

            return PartialView("_ResultatsRechercheLivres", result);
        }
    }
}
