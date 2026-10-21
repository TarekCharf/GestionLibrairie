using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetLibrairie.Migrations
{
    /// <inheritdoc />
    public partial class InitialMySql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adherents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nom = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    prenom = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    login = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mot_de_passe_hash = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_fin_bannissement = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adherents", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "administrateurs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    login = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mot_de_passe_hash = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_administrateurs", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nom = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "livres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    titre = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    auteur = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    categorie_id = table.Column<int>(type: "int", nullable: false),
                    disponible = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_livres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_livres_categories_categorie_id",
                        column: x => x.categorie_id,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "commentaires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    texte = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_creation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    date_modification = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    livre_id = table.Column<int>(type: "int", nullable: false),
                    adherent_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commentaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_commentaires_adherents_adherent_id",
                        column: x => x.adherent_id,
                        principalTable: "adherents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_commentaires_livres_livre_id",
                        column: x => x.livre_id,
                        principalTable: "livres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "emprunts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    livre_id = table.Column<int>(type: "int", nullable: false),
                    adherent_id = table.Column<int>(type: "int", nullable: false),
                    date_emprunt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    date_retour = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emprunts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_emprunts_adherents_adherent_id",
                        column: x => x.adherent_id,
                        principalTable: "adherents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_emprunts_livres_livre_id",
                        column: x => x.livre_id,
                        principalTable: "livres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ratings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    valeur = table.Column<float>(type: "float", nullable: false),
                    date_notation = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    livre_id = table.Column<int>(type: "int", nullable: false),
                    adherent_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ratings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ratings_adherents_adherent_id",
                        column: x => x.adherent_id,
                        principalTable: "adherents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ratings_livres_livre_id",
                        column: x => x.livre_id,
                        principalTable: "livres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_adherents_email",
                table: "adherents",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_adherents_login",
                table: "adherents",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_administrateurs_login",
                table: "administrateurs",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_commentaires_adherent_id",
                table: "commentaires",
                column: "adherent_id");

            migrationBuilder.CreateIndex(
                name: "IX_commentaires_livre_id",
                table: "commentaires",
                column: "livre_id");

            migrationBuilder.CreateIndex(
                name: "IX_emprunts_adherent_id",
                table: "emprunts",
                column: "adherent_id");

            migrationBuilder.CreateIndex(
                name: "IX_emprunts_livre_id",
                table: "emprunts",
                column: "livre_id");

            migrationBuilder.CreateIndex(
                name: "IX_livres_categorie_id",
                table: "livres",
                column: "categorie_id");

            migrationBuilder.CreateIndex(
                name: "IX_ratings_adherent_id",
                table: "ratings",
                column: "adherent_id");

            migrationBuilder.CreateIndex(
                name: "IX_ratings_livre_id_adherent_id",
                table: "ratings",
                columns: new[] { "livre_id", "adherent_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "administrateurs");

            migrationBuilder.DropTable(
                name: "commentaires");

            migrationBuilder.DropTable(
                name: "emprunts");

            migrationBuilder.DropTable(
                name: "ratings");

            migrationBuilder.DropTable(
                name: "adherents");

            migrationBuilder.DropTable(
                name: "livres");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
