using System.Collections.Generic;

namespace ProjetLibrairie.ViewModels
{
    public class StatistiquesViewModel
    {
        // KPIs
        public int EmpruntsActifs { get; set; }
        public int LivresDisponibles { get; set; }
        public int LivresEmpruntes { get; set; }
        public int NombreAdherents { get; set; }

        // Tops
        public List<TopLivreItem> TopLivres { get; set; } = new();
        public List<TopAdherentItem> TopAdherents { get; set; } = new();
        public List<TopCategorieItem> TopCategories { get; set; } = new();
        public List<TopLivreRatingItem> TopLivresRatings { get; set; } = new();

    }

    public class TopLivreItem
    {
        public int LivreId { get; set; }
        public string Titre { get; set; } = "";
        public string Auteur { get; set; } = "";
        public int NbEmprunts { get; set; }
    }

    public class TopAdherentItem
    {
        public int AdherentId { get; set; }
        public string NomComplet { get; set; } = "";
        public string Email { get; set; } = "";
        public int NbEmprunts { get; set; }
    }

    public class TopCategorieItem
    {
        public int CategorieId { get; set; }
        public string Nom { get; set; } = "";
        public int NbEmprunts { get; set; }
    }

    public class TopLivreRatingItem
    {
        public int LivreId { get; set; }
        public string Titre { get; set; } = "";
        public string Auteur { get; set; } = "";
        public double NoteMoyenne { get; set; }
        public int NbRatings { get; set; }
    }
}