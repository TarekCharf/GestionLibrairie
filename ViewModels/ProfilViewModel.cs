using System.ComponentModel.DataAnnotations;

namespace ProjetLibrairie.ViewModels
{
    public class ProfilViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le login est obligatoire.")]
        [Display(Name = "Login")]
        [StringLength(20)]
        public string Login { get; set; } = "";

        [Display(Name = "Nouveau mot de passe")]
        [DataType(DataType.Password)]
        [StringLength(100)]
        public string? NouveauMotDePasse { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare(nameof(NouveauMotDePasse), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string? ConfirmerNouveauMotDePasse { get; set; }

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