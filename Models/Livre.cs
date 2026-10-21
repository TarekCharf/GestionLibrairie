using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetLibrairie.Models
{
    public class Livre
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le titre est obligatoire.")]
        [StringLength(60)]
        [Column("titre")]
        public string Titre { get; set; } = "";

        [Required(ErrorMessage = "L'auteur est obligatoire.")]
        [StringLength(20)]
        [Column("auteur")]
        public string Auteur { get; set; } = "";

        [Required(ErrorMessage = "La catégorie est obligatoire.")]
        [Column("categorie_id")]
        public int CategorieId { get; set; }
        public Categorie? Categorie { get; set; }

        [Column("disponible")]
        public bool Disponible { get; set; } = true;

        // Commentaires écrits par cet adhérent
        public ICollection<Commentaire> Commentaires { get; set; } = new List<Commentaire>();

        // Notes (ratings) données par cet adhérent
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
