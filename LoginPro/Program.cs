using LoginPro.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
//connectionString appsettings.json'a ta��nd�
builder.Services.AddDbContext<DatabaseContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    //opts.UseSqlServer("Server=PC-KOYUNCUOGLU;Database=LoginProMB;Trusted_Connection=true");////connectionString appsettings.json'a ta��nd�
    //opts.UseLazyLoadingProxies();//�imdilik tek tablo oldu�u i�in aktifle�tirilmedi
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts =>
    {
        opts.Cookie.Name = "LoginPro.auth";
        //g�ncellenen yaz�l�m�n kullan�c�ya yans�mas� i�in cookie 1 ay 1 y�l gibi bir s�re olmamal�
        //cookie'de belirli bir limit var �ok b�y�k bir veri tutma
        opts.ExpireTimeSpan = TimeSpan.FromDays(1);
        //opts.SlidingExpiration = true;//sistem kullan�ld�k�a cookie uzar
        opts.LoginPath = "/Account/Login";
        opts.LogoutPath = "/Account/Logout";
        opts.AccessDeniedPath = "/Home/AccessDenied";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();//eklemeyi unutma

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
