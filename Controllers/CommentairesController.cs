using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetLibrairie.Data;
using ProjetLibrairie.Helpers;
using ProjetLibrairie.Models;

namespace ProjetLibrairie.Controllers
{
    public class CommentairesController : Controller
    {
        private readonly BiblioDbContext _db;
        public CommentairesController(BiblioDbContext db) => _db = db;

        // 1. Liste globale de tous les commentaires (Modération - Réservé Admin)
        [RequireRole(AppRole.Admin)]
        public async Task<IActionResult> Index()
        {
            var commentaires = await _db.Commentaires
                .AsNoTracking()
                .Include(c => c.Livre)
                .Include(c => c.Adherent)
                .OrderByDescending(c => c.DateCreation)
                .ToListAsync();

            return View(commentaires);
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

            var commentaires = await _db.Commentaires
                .AsNoTracking()
                .Include(c => c.Adherent)
                .Where(c => c.LivreId == livreId)
                .OrderByDescending(c => c.DateCreation)
                .ToListAsync();

            ViewBag.Livre = livre;
            return View(commentaires); // La vue attend un IEnumerable<Commentaire>
        }

        // 3. Formulaire de création (GET) - Généralement réservé aux Adhérents
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

            // 3. NOUVEAU : Vérifier si l'adhérent a déjà écrit un commentaire sur CE livre
            var dejaCommente = await _db.Commentaires
                .AnyAsync(c => c.LivreId == livreId && c.AdherentId == currentAdherentId.Value);

            if (dejaCommente)
            {
                TempData["ErrorMessage"] = "Vous avez déjà publié un commentaire sur ce livre. Vous pouvez uniquement le modifier.";
                return RedirectToAction("Details", "Livres", new { id = livreId });
            }

            var commentaire = new Commentaire { LivreId = livreId };
            ViewBag.LivreTitre = livre.Titre;
            return View(commentaire);
        }

        // 4. Traitement de la création (POST)
        [RequireRole(AppRole.Adherent)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Creer(Commentaire commentaire)
        {
            // Récupération automatique de l'ID de l'adhérent connecté
            int? currentAdherentId = AuthSession.AdherentId(HttpContext);
            if (currentAdherentId == null) return Unauthorized();

            // NOUVEAU : Double sécurité côté serveur (anti-doublon)
            var dejaCommente = await _db.Commentaires
                .AnyAsync(c => c.LivreId == commentaire.LivreId && c.AdherentId == currentAdherentId.Value);

            if (dejaCommente)
            {
                ModelState.AddModelError(string.Empty, "Vous avez déjà soumis un commentaire pour cet ouvrage.");
                return View(commentaire);
            }

            // Attribution forcée des valeurs système (sécurité)
            commentaire.AdherentId = currentAdherentId.Value;
            commentaire.DateCreation = DateTime.Now;

            // On retire les objets de navigation de la validation pour éviter les blocages du ModelState
            ModelState.Remove(nameof(commentaire.Livre));
            ModelState.Remove(nameof(commentaire.Adherent));

            if (!ModelState.IsValid) return View(commentaire);

            _db.Commentaires.Add(commentaire);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Commentaire créé avec succès.";
            // Redirection vers les détails du livre où le commentaire a été déposé
            return RedirectToAction("Details", "Livres", new { id = commentaire.LivreId });
        }

        // 5. Formulaire de modification (GET)
        [RequireRole(AppRole.Adherent)]
        [HttpGet]
        public async Task<IActionResult> Modifier(int? id)
        {
            if (id == null) return NotFound();

            var commentaire = await _db.Commentaires.Include(c => c.Livre).FirstOrDefaultAsync(c => c.Id == id.Value);
            if (commentaire == null) return NotFound();

            // SÉCURITÉ : L'adhérent ne peut modifier que son PROPRE commentaire (l'admin outrepasse ce droit)
            if (!AuthSession.EstAdmin(HttpContext) && commentaire.AdherentId != AuthSession.AdherentId(HttpContext))
            {
                return Forbid(); // Accès refusé
            }

            return View(commentaire);
        }

        // 6. Traitement de la modification (POST)
        [RequireRole(AppRole.Adherent)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modifier(int id, string texte)
        {
            var commentaireExistant = await _db.Commentaires.FindAsync(id);
            if (commentaireExistant == null) return NotFound();

            // SÉCURITÉ : Vérification des droits de propriété
            if (!AuthSession.EstAdmin(HttpContext) && commentaireExistant.AdherentId != AuthSession.AdherentId(HttpContext))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(texte))
            {
                ModelState.AddModelError(nameof(texte), "Le texte du commentaire ne peut pas être vide.");
                return View(commentaireExistant);
            }

            // On met à jour uniquement la colonne modifiable (évite d'écraser la date ou l'auteur)
            commentaireExistant.Texte = texte;

            _db.Commentaires.Update(commentaireExistant);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Commentaire modifié avec succès.";
            return RedirectToAction("Details", "Livres", new { id = commentaireExistant.LivreId });
        }

        // 7. Formulaire de confirmation de suppression (GET)
        [RequireRole(AppRole.Adherent)]
        [HttpGet]
        public async Task<IActionResult> Supprimer(int? id)
        {
            if (id == null) return NotFound();

            var commentaire = await _db.Commentaires.Include(c => c.Livre).FirstOrDefaultAsync(c => c.Id == id.Value);
            if (commentaire == null) return NotFound();

            // SÉCURITÉ : Seul l'auteur ou l'admin peut supprimer
            if (!AuthSession.EstAdmin(HttpContext) && commentaire.AdherentId != AuthSession.AdherentId(HttpContext))
            {
                return Forbid();
            }

            return View(commentaire);
        }

        // 8. Traitement de la suppression (POST)
        [RequireRole(AppRole.Adherent)]
        [HttpPost, ActionName("Supprimer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupprimerConfirme(int id)
        {
            var commentaire = await _db.Commentaires.FindAsync(id);
            if (commentaire == null) return NotFound();

            // SÉCURITÉ : Vérification finale des droits
            if (!AuthSession.EstAdmin(HttpContext) && commentaire.AdherentId != AuthSession.AdherentId(HttpContext))
            {
                return Forbid();
            }

            int sauvegardeLivreId = commentaire.LivreId;

            _db.Commentaires.Remove(commentaire);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Commentaire supprimé avec succès.";

            // Si c'est l'admin qui supprime depuis l'Index global, on le renvoie à l'Index, sinon vers le livre
            if (AuthSession.EstAdmin(HttpContext))
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("Details", "Livres", new { id = sauvegardeLivreId });
        }
    }
}