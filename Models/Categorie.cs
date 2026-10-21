using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetLibrairie.Models
{
    [Table("categories")]
    public class Categorie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom de la catégorie est obligatoire.")]
        [StringLength(50)]
        [Column("nom")]
        public string Nom { get; set; } = "";

        // Navigation
        public ICollection<Livre> Livres { get; set; } = new List<Livre>();
    }
}