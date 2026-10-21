using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetLibrairie.Data;
using ProjetLibrairie.Helpers;
using ProjetLibrairie.Models;
using ProjetLibrairie.ViewModels;

namespace ProjetLibrairie.Controllers
{
    public class CompteController : Controller
    {
        private readonly BiblioDbContext _db;
        public CompteController(BiblioDbContext db) => _db = db;

        // -------------------------
        // Connexion
        // -------------------------
        [HttpGet]
        public IActionResult Connexion()
        {
            // Par défaut: Admin (comme avant). Tu peux mettre "Adherent" si tu veux.
            return View(new LoginViewModel { Role = "Admin" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Connexion(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            if (string.IsNullOrWhiteSpace(vm.Identifiant))
            {
                ModelState.AddModelError(nameof(vm.Identifiant), "Identifiant obligatoire.");
                return View(vm);
            }

            if (string.IsNullOrWhiteSpace(vm.MotDePasse))
            {
                ModelState.AddModelError(nameof(vm.MotDePasse), "Mot de passe obligatoire.");
                return View(vm);
            }

            if (vm.Role == "Admin")
            {
                var admin = await _db.Administrateurs
                    .FirstOrDefaultAsync(a => a.Login == vm.Identifiant);

                if (admin == null || !BCrypt.Net.BCrypt.Verify(vm.MotDePasse, admin.MotDePasseHash))
                {
                    TempData["Error"] = "Login ou mot de passe incorrect.";
                    ModelState.AddModelError("", "Login ou mot de passe incorrect.");
                    return View(vm);
                }
                HttpContext.Session.SetInt32(AuthSession.AdminIdKey, admin.Id);
                HttpContext.Session.SetString(AuthSession.RoleKey, "Admin");
                HttpContext.Session.SetString(AuthSession.AdminLoginKey, admin.Login);
                TempData["Success"] = "Connexion réussie.";
                return RedirectToAction("Index", "Accueil");
            }
            else // Adherent
            {
                // Maintenant l'adhérent se connecte par Login + MotDePasse
                var adh = await _db.Adherents
                    .FirstOrDefaultAsync(a => a.Login == vm.Identifiant);

                if (adh == null || !BCrypt.Net.BCrypt.Verify(vm.MotDePasse, adh.MotDePasseHash))
                {
                    TempData["Error"] = "Login ou mot de passe incorrect.";
                    ModelState.AddModelError("", "Login ou mot de passe incorrect.");
                    return View(vm);
                }

                if (adh.EstBanni)
                {
                    TempData["Error"] = $"Compte banni jusqu'au {adh.DateFinBannissement:yyyy-MM-dd}.";
                    ModelState.AddModelError("", "Compte banni.");
                    return View(vm);
                }

                HttpContext.Session.SetInt32(AuthSession.AdherentIdKey, adh.Id);
                HttpContext.Session.SetString(AuthSession.RoleKey, "Adherent");
                HttpContext.Session.SetString(AuthSession.AdherentNomKey, adh.Nom);
                HttpContext.Session.SetString(AuthSession.AdherentPrenomKey, adh.Prenom);
                TempData["Success"] = "Connexion réussie.";
                return RedirectToAction("MesEmprunts", "Emprunts");
            }
        }

        // -------------------------
        // Inscription (Adhérent uniquement)
        // -------------------------
        [HttpGet]
        public IActionResult Inscription()
        {
            return View(new InscriptionViewModel { Role = "Adherent" }); // role fixé
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscription(InscriptionViewModel vm)
        {
            // on force Adherent, même si quelqu'un modifie le HTML
            vm.Role = "Adherent";

            // validations
            if (string.IsNullOrWhiteSpace(vm.Nom))
                ModelState.AddModelError(nameof(vm.Nom), "Le nom est obligatoire.");

            if (string.IsNullOrWhiteSpace(vm.Prenom))
                ModelState.AddModelError(nameof(vm.Prenom), "Le prénom est obligatoire.");

            if (string.IsNullOrWhiteSpace(vm.Email))
                ModelState.AddModelError(nameof(vm.Email), "L'email est obligatoire.");

            if (string.IsNullOrWhiteSpace(vm.Login))
                ModelState.AddModelError(nameof(vm.Login), "Le login est obligatoire.");

            if (string.IsNullOrWhiteSpace(vm.MotDePasse))
                ModelState.AddModelError(nameof(vm.MotDePasse), "Le mot de passe est obligatoire.");

            if (!ModelState.IsValid) return View(vm);

            // unicité email + login
            bool emailExiste = await _db.Adherents.AnyAsync(a => a.Email == vm.Email);
            if (emailExiste)
            {
                ModelState.AddModelError(nameof(vm.Email), "Cet email existe déjà.");
                return View(vm);
            }

            bool loginExiste = await _db.Adherents.AnyAsync(a => a.Login == vm.Login);
            if (loginExiste)
            {
                ModelState.AddModelError(nameof(vm.Login), "Ce login existe déjà.");
                return View(vm);
            }

            var adherent = new Adherent
            {
                Nom = vm.Nom!.Trim(),
                Prenom = vm.Prenom!.Trim(),
                Email = vm.Email!.Trim(),
                Login = vm.Login!.Trim(),
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(vm.MotDePasse!)
            };

            _db.Adherents.Add(adherent);
            await _db.SaveChangesAsync();

            // Connexion auto
            HttpContext.Session.SetInt32(AuthSession.AdherentIdKey, adherent.Id);
            HttpContext.Session.SetString(AuthSession.RoleKey, "Adherent");
            HttpContext.Session.SetString(AuthSession.AdherentNomKey, adherent.Nom);
            HttpContext.Session.SetString(AuthSession.AdherentPrenomKey, adherent.Prenom);
            TempData["Success"] = "Inscription réussie. Bienvenue !";
            return RedirectToAction("MesEmprunts", "Emprunts");
        }

        // -------------------------
        // Déconnexion
        // -------------------------
        public IActionResult Deconnexion()
        {
            AuthSession.Deconnecter(HttpContext);
            TempData["Info"] = "Vous avez été déconnecté.";
            return RedirectToAction("Connexion");
        }
    }
}