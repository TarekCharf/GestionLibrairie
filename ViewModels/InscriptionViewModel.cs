using System.ComponentModel.DataAnnotations;

namespace ProjetLibrairie.ViewModels
{
    public class InscriptionViewModel
    {
        [Required(ErrorMessage = "Le rôle est obligatoire.")]
        [Display(Name = "Rôle")]
        public string Role { get; set; } = "Adherent";

        [Required(ErrorMessage = "Le login est obligatoire.")]
        [Display(Name = "Login")]
        [StringLength(20)]
        public string Login { get; set; } = "";

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        [StringLength(100)]
        public string MotDePasse { get; set; } = "";

        [Required(ErrorMessage = "La confirmation du mot de passe est obligatoire.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare(nameof(MotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmerMotDePasse { get; set; } = "";

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [Display(Name = "Nom")]
        [StringLength(20)]
        public string Nom { get; set; } = "";

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [Display(Name = "Prénom")]
        [StringLength(20)]
        public string Prenom { get; set; } = "";

        [Required(ErrorMessage = "L'email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Email invalide.")]
        [Display(Name = "Email")]
        [StringLength(50)]
        public string Email { get; set; } = "";
    }
}