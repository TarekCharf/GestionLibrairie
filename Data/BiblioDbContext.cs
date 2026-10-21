using ProjetLibrairie.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjetLibrairie.Data
{
    public class BiblioDbContext : DbContext
    {
        public BiblioDbContext(DbContextOptions<BiblioDbContext> options) : base(options) { }

        public DbSet<Categorie> Categories => Set<Categorie>();
        public DbSet<Livre> Livres => Set<Livre>();
        public DbSet<Adherent> Adherents => Set<Adherent>();
        public DbSet<Emprunt> Emprunts => Set<Emprunt>();
        public DbSet<Administrateur> Administrateurs => Set<Administrateur>();
        public DbSet<Commentaire> Commentaires => Set<Commentaire>();
        public DbSet<Rating> Ratings => Set<Rating>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Mappage des tables
            modelBuilder.Entity<Categorie>().ToTable("categories");
            modelBuilder.Entity<Livre>().ToTable("livres");
            modelBuilder.Entity<Adherent>().ToTable("adherents");
            modelBuilder.Entity<Emprunt>().ToTable("emprunts");
            modelBuilder.Entity<Administrateur>().ToTable("administrateurs");
            modelBuilder.Entity<Commentaire>().ToTable("commentaires");
            modelBuilder.Entity<Rating>().ToTable("ratings");

            // 2. Relations de l'entité LIVRE
            modelBuilder.Entity<Livre>()
                .HasOne(l => l.Categorie)
                .WithMany(c => c.Livres)
                .HasForeignKey(l => l.CategorieId);

            // 3. Relations de l'entité COMMENTAIRE
            modelBuilder.Entity<Commentaire>()
                .HasOne(c => c.Livre)
                .WithMany(l => l.Commentaires)
                .HasForeignKey(c => c.LivreId);

            modelBuilder.Entity<Commentaire>() // <-- AJOUT : Relation avec l'Adhérent
                .HasOne(c => c.Adherent)
                .WithMany(a => a.Commentaires)
                .HasForeignKey(c => c.AdherentId);

            // 4. Relations et contraintes de l'entité RATING
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Livre)
                .WithMany(l => l.Ratings)
                .HasForeignKey(r => r.LivreId);

            modelBuilder.Entity<Rating>() // <-- AJOUT : Relation avec l'Adhérent
                .HasOne(r => r.Adherent)
                .WithMany(a => a.Ratings)
                .HasForeignKey(r => r.AdherentId);

            // Contrainte : Un adhérent ne met qu'une seule note par livre
            modelBuilder.Entity<Rating>()
                .HasIndex(r => new { r.LivreId, r.AdherentId })
                .IsUnique();

            // 5. Relations de l'entité EMPRUNT
            modelBuilder.Entity<Emprunt>()
                .HasOne(e => e.Livre)
                .WithMany() // Laissez vide si Livre n'a pas de propriété "ICollection<Emprunt>"
                .HasForeignKey(e => e.LivreId)
                .OnDelete(DeleteBehavior.Restrict); // <-- AJOUT : Évite de supprimer un livre s'il a été emprunté

            modelBuilder.Entity<Emprunt>()
                .HasOne(e => e.Adherent)
                .WithMany() // Mettez ".WithMany(a => a.Emprunts)" si vous ajoutez la collection dans Adherent.cs
                .HasForeignKey(e => e.AdherentId)
                .OnDelete(DeleteBehavior.Restrict); // <-- AJOUT : Évite de supprimer un adhérent s'il a un historique d'emprunts

            // 6. Index uniques pour la sécurité (Authentification / Identification)
            modelBuilder.Entity<Adherent>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<Adherent>() // <-- AJOUT : Sécurité sur le login de l'adhérent
                .HasIndex(a => a.Login)
                .IsUnique();

            modelBuilder.Entity<Administrateur>()
                .HasIndex(a => a.Login)
                .IsUnique();
        }
    }
}