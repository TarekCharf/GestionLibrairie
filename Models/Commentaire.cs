using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetLibrairie.Models
{
    [Table("commentaires")]
    public class Commentaire
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le contenu du commentaire est obligatoire.")]
        [StringLength(1000, ErrorMessage = "Le commentaire ne peut pas dépasser 1000 caractères.")]
        [Column("texte")]
        public string Texte { get; set; } = "";

        [Required]
        [Column("date_creation")]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Column("date_modification")]
        public DateTime? DateModification { get; set; }

        // Relation avec le Livre
        [Required]
        [Column("livre_id")]
        public int LivreId { get; set; }
        public Livre? Livre { get; set; }

        // Relation avec l'Adhérent
        [Required]
        [Column("adherent_id")]
        public int AdherentId { get; set; }
        public Adherent? Adherent { get; set; }
    }
}