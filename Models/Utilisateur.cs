using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetLibrairie.Models
{
    [NotMapped] // important: pas de table "base"
    public abstract class Utilisateur
    {
        [Required(ErrorMessage = "Le login est obligatoire.")]
        [StringLength(20)]
        public virtual string Login { get; set; } = "";

        [Required(ErrorMessage = "Le mot de passe est obligatoire.")]
        [StringLength(200)] // hash bcrypt peut être long (~60) -> 200 safe
        public virtual string MotDePasseHash { get; set; } = "";
    }
}