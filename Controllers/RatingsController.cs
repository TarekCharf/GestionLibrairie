using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetLibrairie.Data;
using ProjetLibrairie.Helpers;
using ProjetLibrairie.Models;

namespace ProjetLibrairie.Controllers
{
    public class RatingsController : Controller
    {
        private readonly BiblioDbContext _db;
        public RatingsController(BiblioDbContext db) => _db = db;

        // 1. Liste globale de tous les commentaires (Modération - Réservé Admin)
        [RequireRole(AppRole.Admin)]
        public async Task<IActionResult> Index()
        {
            var ratings = await _db.Ratings
                .AsNoTracking()
                .Include(c => c.Livre)
                .Include(c => c.Adherent)
                .OrderByDescending(c => c.DateNotation)
                .ToListAsync();

            return View(ratings);
        }

        // 2. Afficher TOUS les commentaires d'un livre spécifique
        [RequireRole(AppRole.Admin, AppRole.Adherent)]
        [HttpGet]
        public async Task<IActionResult> IndexLivre(int livreId)
        {
            var livre = await _db.Livres.FindAsync(livreId);
            if (livre == null)
            {
                TempData["ErrorMessage"] = "Le livre spécifié n'existe pas.";
                return RedirectToAction("Index", "Livres");
            }

            var ratings = await _db.Ratings
                .AsNoTracking()
                .Include(c => c.Adherent)
                .Where(c => c.LivreId == livreId)
                .OrderByDescending(c => c.DateNotation)
                .ToListAsync();

            ViewBag.Livre = livre;
            return View(ratings); // Note: Assurez-vous que la vue utilise @model IEnumerable<Rating>
        }

        // 3. Formulaire de création (GET)
        [RequireRole(AppRole.Adherent)]
        [HttpGet]
        public async Task<IActionResult> Creer(int livreId)
        {
            // 1. Récupération de l'adhérent connecté
            int? currentAdherentId = AuthSession.AdherentId(HttpContext);
            if (currentAdherentId == null) return Unauthorized();

            // 2. Vérification de l'existence du livre
            var livre = await _db.Livres.FindAsync(livreId);
            if (livre == null)
            {
                TempData["ErrorMessage"] = "Le livre n'existe pas.";
                return RedirectToAction("Index", "Livres");
            }

            // 3. NOUVEAU : Vérifier si l'adhérent a déjà noté CE livre
            var dejaNote = await _db.Ratings
                .AnyAsync(r => r.LivreId == livreId && r.AdherentId == currentAdherentId.Value);

            if (dejaNote)
            {
                TempData["ErrorMessage"] = "Vous avez déjà attribué une note à ce livre. Vous pouvez uniquement la modifier.";
                return RedirectToAction("Details", "Livres", new { id = livreId });
            }

            var ratings = new Rating { LivreId = livreId };
            ViewBag.LivreTitre = livre.Titre;
            return View(ratings);
        }

        // 4. Traitement de la création (POST)
        [RequireRole(AppRole.Adherent)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Creer(Rating rating)
        {
            int? currentAdherentId = AuthSession.AdherentId(HttpContext);
            if (currentAdherentId == null) return Unauthorized();

            // NOUVEAU : Double sécurité côté serveur (anti-doublon)
            var dejaNote = await _db.Ratings
                .AnyAsync(r => r.LivreId == rating.LivreId && r.AdherentId == currentAdherentId.Value);

            if (dejaNote)
            {
                ModelState.AddModelError(string.Empty, "Vous avez déjà soumis une note pour cet ouvrage.");

                // Recharger le titre du livre avant de retourner la vue
                var livre = await _db.Livres.FindAsync(rating.LivreId);
                if (livre != null) ViewBag.LivreTitre = livre.Titre;

                return View(rating);
            }

            rating.AdherentId = currentAdherentId.Value;
            rating.DateNotation = DateTime.Now;

            ModelState.Remove(nameof(rating.Livre));
            ModelState.Remove(nameof(rating.Adherent));

            if (!ModelState.IsValid)
            {
                // Recharger le titre du livre en cas d'échec de validation pour la vue
                var livre = await _db.Livres.FindAsync(rating.LivreId);
                if (livre != null) ViewBag.LivreTitre = livre.Titre;
                return View(rating);
            }

            _db.Ratings.Add(rating);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Rating créé avec succès.";
            return RedirectToAction("Details", "Livres", new { id = rating.LivreId });
        }

        // 5. Formulaire de modification (GET)
        [RequireRole(AppRole.Adherent)]
        [HttpGet]
        public async Task<IActionResult> Modifier(int? id)
        {
            if (id == null) return NotFound();

            var rating = await _db.Ratings.Include(c => c.Livre).FirstOrDefaultAsync(c => c.Id == id.Value);
            if (rating == null) return NotFound();

            if (!AuthSession.EstAdmin(HttpContext) && rating.AdherentId != AuthSession.AdherentId(HttpContext))
            {
                return Forbid();
            }

            return View(rating);
        }

        // 6. Traitement de la modification (POST)
        [RequireRole(AppRole.Adherent)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modifier(int id, int valeur)
        {
            // Correction : On inclut d'emblée 'Livre' pour parer au risque de crash si on doit retourner à la View
            var ratingExistant = await _db.Ratings.Include(c => c.Livre).FirstOrDefaultAsync(c => c.Id == id);
            if (ratingExistant == null) return NotFound();

            if (!AuthSession.EstAdmin(HttpContext) && ratingExistant.AdherentId != AuthSession.AdherentId(HttpContext))
            {
                return Forbid();
            }

            // Correction : Exemple de validation de plage de note si nécessaire (ex: entre 1 et 5)
            if (valeur <= 0 || valeur > 5)
            {
                // Correction : Utilisation du nom de propriété correct pour la correspondance dans la vue (Casse)
                ModelState.AddModelError(nameof(ratingExistant.Valeur), "La valeur du rating doit être comprise entre 1 et 5.");
                return View(ratingExistant);
            }

            ratingExistant.Valeur = valeur;

            _db.Ratings.Update(ratingExistant);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Rating modifié avec succès.";
            return RedirectToAction("Details", "Livres", new { id = ratingExistant.LivreId });
        }

        // 7. Formulaire de confirmation de suppression (GET)
        [RequireRole(AppRole.Adherent)]
        [HttpGet]
        public async Task<IActionResult> Supprimer(int? id)
        {
            if (id == null) return NotFound();

            var rating = await _db.Ratings.Include(c => c.Livre).FirstOrDefaultAsync(c => c.Id == id.Value);
            if (rating == null) return NotFound();

            if (!AuthSession.EstAdmin(HttpContext) && rating.AdherentId != AuthSession.AdherentId(HttpContext))
            {
                return Forbid();
            }

            return View(rating);
        }

        // 8. Traitement de la suppression (POST)
        [RequireRole(AppRole.Adherent)]
        [HttpPost, ActionName("Supprimer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupprimerConfirme(int id)
        {
            var rating = await _db.Ratings.FindAsync(id);
            if (rating == null) return NotFound();

            if (!AuthSession.EstAdmin(HttpContext) && rating.AdherentId != AuthSession.AdherentId(HttpContext))
            {
                return Forbid();
            }

            int sauvegardeLivreId = rating.LivreId;

            _db.Ratings.Remove(rating);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Rating supprimé avec succès.";

            if (AuthSession.EstAdmin(HttpContext))
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Details", "Livres", new { id = sauvegardeLivreId });
        }
    }
}