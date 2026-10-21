namespace ProjetLibrairie.Helpers
{
    public static class AuthSession
    {
        public const string AdminIdKey = "AdminId";
        public const string AdherentIdKey = "AdherentId";
        public const string RoleKey = "Role";
        public const string AdminLoginKey = "AdminLogin";
        public const string AdherentNomKey = "AdherentNom";
        public const string AdherentPrenomKey = "AdherentPrenom";

        public static bool EstAdmin(HttpContext ctx) => ctx.Session.GetInt32(AdminIdKey) != null;
        public static bool EstAdherent(HttpContext ctx) => ctx.Session.GetInt32(AdherentIdKey) != null;

        public static int? AdminId(HttpContext ctx) => ctx.Session.GetInt32(AdminIdKey);
        public static int? AdherentId(HttpContext ctx) => ctx.Session.GetInt32(AdherentIdKey);
        public static string? AdminLogin(HttpContext ctx) => ctx.Session.GetString(AdminLoginKey);
        public static string? AdherentNom(HttpContext ctx) => ctx.Session.GetString(AdherentNomKey);
        public static string? AdherentPrenom(HttpContext ctx) => ctx.Session.GetString(AdherentPrenomKey);
        public static void Deconnecter(HttpContext ctx)
        {
            ctx.Session.Clear();
        }
    }
}
