using LoginPro.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
//connectionString appsettings.json'a taþýndý
builder.Services.AddDbContext<DatabaseContext>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    //opts.UseSqlServer("Server=PC-KOYUNCUOGLU;Database=LoginProMB;Trusted_Connection=true");////connectionString appsettings.json'a taþýndý
    //opts.UseLazyLoadingProxies();//þimdilik tek tablo olduðu için aktifleþtirilmedi
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opts =>
    {
        opts.Cookie.Name = "LoginPro.auth";
        //güncellenen yazýlýmýn kullanýcýya yansýmasý için cookie 1 ay 1 yýl gibi bir süre olmamalý
        //cookie'de belirli bir limit var çok büyük bir veri tutma
        opts.ExpireTimeSpan = TimeSpan.FromDays(1);
        //opts.SlidingExpiration = true;//sistem kullanýldýkça cookie uzar
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
