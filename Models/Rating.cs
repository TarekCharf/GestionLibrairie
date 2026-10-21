using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetLibrairie.Models
{
    [Table("ratings")]
    public class Rating
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La note est obligatoire.")]
        [Range(0, 5, ErrorMessage = "La note doit être comprise entre 0 et 5 étoiles.")]
        [Column("valeur")]
        public float Valeur { get; set; }

        [Required]
        [Column("date_notation")]
        public DateTime DateNotation { get; set; } = DateTime.Now;

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