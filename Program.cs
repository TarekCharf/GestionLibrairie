using Microsoft.EntityFrameworkCore;
using ProjetLibrairie.Data;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();


builder.Services.AddDbContext<BiblioDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("BiblioDb");
    options.UseMySql(cs, ServerVersion.AutoDetect(cs));
});


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Erreur");
    app.UseStatusCodePagesWithReExecute("/Erreur/{0}");
    app.UseHsts();
}

RotativaConfiguration.Setup(
    app.Environment.WebRootPath,
    "Rotativa"
);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting(); 

app.UseSession(); 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Compte}/{action=Connexion}/{id?}");

app.Run();