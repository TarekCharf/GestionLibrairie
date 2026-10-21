using System.ComponentModel.DataAnnotations.Schema;

namespace ProjetLibrairie.Models
{
    [Table("administrateurs")]
    public class Administrateur : Utilisateur
    {
        public int Id { get; set; }

        [Column("login")]
        public override string Login { get; set; } = "";

        [Column("mot_de_passe_hash")]
        public override string MotDePasseHash { get; set; } = "";

    }
}