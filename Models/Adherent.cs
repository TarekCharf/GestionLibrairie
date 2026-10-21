using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetLibrairie.Models
{
    [Table("adherents")]
    public class Adherent : Utilisateur
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire.")]
        [StringLength(20)]
        [Column("nom")]
        public string Nom { get; set; } = "";

        [Required(ErrorMessage = "Le prénom est obligatoire.")]
        [StringLength(20)]
        [Column("prenom")]
        public string Prenom { get; set; } = "";

        [Required(ErrorMessage = "L'email est obligatoire.")]
        [EmailAddress(ErrorMessage = "Email invalide.")]
        [StringLength(50)]
        [Column("email")]
        public string Email { get; set; } = "";

        // Hérités (mappés sur la table adherents)
        [Column("login")]
        public override string Login { get; set; } = "";

        [Column("mot_de_passe_hash")]
        public override string MotDePasseHash { get; set; } = "";

        [Column("date_fin_bannissement")]
        public DateTime? DateFinBannissement { get; set; }

        [NotMapped]
        public bool EstBanni => DateFinBannissement.HasValue && DateFinBannissement.Value > DateTime.Now;

        // Liste des commentaires sur ce livre
        public ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();

        // Liste des notes (ratings) attribuées à ce livre
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    }
}