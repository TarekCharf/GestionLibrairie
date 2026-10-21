using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetLibrairie.Models
{
    [Table("emprunts")]
    public class Emprunt
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le Id est obligatoire.")]
        [Column("livre_id")]
        public int LivreId { get; set; }

        [Required(ErrorMessage = "L'adherent est obligatoire.")]
        [Column("adherent_id")]
        public int AdherentId { get; set; }

        [Required(ErrorMessage = "La date d'emprunt est obligatoire.")]
        [Column("date_emprunt")]
        public DateTime DateEmprunt { get; set; } = DateTime.Now;

        [Column("date_retour")]
        public DateTime? DateRetour { get; set; }

        public Livre? Livre { get; set; }
        public Adherent? Adherent { get; set; }
    }
}