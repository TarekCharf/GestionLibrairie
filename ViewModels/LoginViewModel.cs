using System.ComponentModel.DataAnnotations;

namespace ProjetLibrairie.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        [Display(Name = "Rôle")]
        public string Role { get; set; } = "Admin"; // Admin / Adherent

        [Required(ErrorMessage = "Login obligatoire.")]
        [Display(Name = "Login")]
        public string Identifiant { get; set; } = "";

        [Required(ErrorMessage = "Mot de passe obligatoire.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string MotDePasse { get; set; } = "";
    }
}