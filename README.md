# GestionLibrairie

Application web ASP.NET Core MVC (NET 8) pour la gestion d'une bibliothèque : gestion des livres, catégories, adhérents, emprunts, commentaires et notation.

## Stack technique

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core 8
- MySQL (Pomelo)
- Sessions ASP.NET Core
- Rotativa / wkhtmltopdf pour la génération PDF

## Prérequis

- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- Un serveur MySQL (local ou Azure Database for MySQL)
- (Optionnel) Visual Studio 2022 / VS Code

## Lancer le projet en local

1. Cloner le dépôt.
2. Configurer la chaîne de connexion `ConnectionStrings:BiblioDb` dans `appsettings.json` (ou via variables d'environnement).
3. Restaurer les dépendances et lancer l'application :

```bash
dotnet restore
dotnet run --project /home/runner/work/GestionLibrairie/GestionLibrairie/ProjetLibrairie.csproj
```

4. Ouvrir l'URL affichée dans la console.

## Déploiement sur Microsoft Azure

> ⚠️ Le projet embarque `wkhtmltopdf.exe` (`wwwroot/Rotativa/wkhtmltopdf.exe`) : utilisez **Azure App Service Windows**.

### 1) Créer les ressources Azure

- Un **Azure Database for MySQL – Flexible Server**
- Un **App Service Plan (Windows)**
- Une **Web App** en **.NET 8 (Windows)**

### 2) Configurer la base MySQL

- Créer la base (ex: `projet_librairie`).
- Autoriser l'accès depuis Azure (règles de pare-feu / accès réseau).
- Noter hôte, port, utilisateur et mot de passe.

### 3) Configurer l'application (Web App)

Dans **App Service > Configuration**, ajouter :

- `ConnectionStrings__BiblioDb` =
  `Server=<serveur>.mysql.database.azure.com;Port=3306;Database=projet_librairie;User=<utilisateur>;******;SslMode=Required;`

Optionnel :
- `ASPNETCORE_ENVIRONMENT=Production`

Puis redémarrer la Web App.

### 4) Publier l'application

#### Option A — Via Visual Studio

- Ouvrir le projet.
- **Publier** > **Azure** > **Azure App Service (Windows)**.
- Sélectionner la Web App créée, puis publier.

#### Option B — Via Azure CLI

```bash
dotnet publish /home/runner/work/GestionLibrairie/GestionLibrairie/ProjetLibrairie.csproj -c Release -o ./publish
cd publish && zip -r ../publish.zip .
az webapp deploy \
  --resource-group <resource_group> \
  --name <webapp_name> \
  --src-path ./publish.zip \
  --type zip
```

> Remarque : créez `publish.zip` à partir du contenu du dossier `publish` avant la commande `az webapp deploy`.

### 5) Appliquer les migrations

Après déploiement, appliquer les migrations EF Core vers la base Azure.

En local (avec la chaîne de connexion Azure) :

```bash
dotnet ef database update --project /home/runner/work/GestionLibrairie/GestionLibrairie/ProjetLibrairie.csproj
```

## Vérification post-déploiement

- L'application démarre sans erreur (`Log stream` App Service).
- La connexion MySQL fonctionne.
- Les fonctionnalités principales (connexion, livres, emprunts, PDF) sont opérationnelles.

## Bonnes pratiques sécurité

- Ne pas commiter de mots de passe dans `appsettings.json`.
- Utiliser les **Application Settings** d'Azure pour les secrets.
- Restreindre les IP autorisées sur le serveur MySQL.
