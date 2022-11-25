using LoginPro.Entities;
using LoginPro.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NETCore.Encrypt.Extensions;
using System.Security.Claims;

namespace LoginPro.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseContext _databaseContext;
        private readonly IConfiguration _configuration;

        public AccountController(DatabaseContext databaseContext, IConfiguration configuration)
        {
            _databaseContext = databaseContext;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string md5Salt = _configuration.GetValue<string>("AppSettings:MD5Salt");
                string saltedPassword = model.Password + md5Salt;
                string hashedPassword = saltedPassword.MD5();
                
                //firstOrDefault kullanılmadı, birden çok varsa hata döndürmesi istenildi
                //aslında aynı username ile ekleme engellenmiş olmasına rağment burada ekstra bir kontrol yapıldı
                User user=_databaseContext.Users.SingleOrDefault(u => u.Username.ToLower() == model.Username.ToLower() && u.Password == saltedPassword);
                if(user != null)
                {
                    if(user.Locked)
                    {
                        ModelState.AddModelError(nameof(model.Username), "user is locked");
                        return View(model);
                    }
                    List<Claim> claims = new List<Claim>();
                    //claims.Add(new Claim("Id",user.Id.ToString()));
                    //claims.Add(new Claim("FullName",user.FullName ?? string.Empty));
                    //claims.Add(new Claim("Username",user.FullName));//username empty olamayacağı için ?? string.Empty vermeye gerek yok
                    //daha global gir yazım için aşağıdaki gibi güncellendi
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                    claims.Add(new Claim(ClaimTypes.Name, user.FullName ?? string.Empty));
                    claims.Add(new Claim("Username", user.FullName));
                    //ClaimsIdentity identity = new ClaimsIdentity(claims,"Cookies");//aşağıdaki gibi düzenlendi
                    ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,principal);
                }
                else
                {
                    ModelState.AddModelError("","username or password incorrect");
                }
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        //post metodu çağrıldığında controller newlenir ve ilgili metod tetiklenir
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_databaseContext.Users.Any(x => x.Username.ToLower() == model.Username.ToLower()))
                {
                    ModelState.AddModelError(nameof(model.Username), "username already exist");
                    View(model);
                }
                string md5Salt = _configuration.GetValue<string>("AppSettings:MD5Salt");
                string saltedPassword = model.Password + md5Salt;
                string hashedPassword = saltedPassword.MD5();

                User user = new()
                {
                    Username = model.Username,
                    //md5 geri dönülmeyecek şekilde bir şifrelemedir
                    //md5'te şifre çözümlenip kontrol yapılmaz. Girilen inputun şifrelenmiş hali ile db'deki şifrelenmiş hali kıyaslanır
                    Password = hashedPassword,
                };
                _databaseContext.Users.Add(user);
                int affectedRowCount = _databaseContext.SaveChanges();//saveChanges int döner: etkilenen row sayısı
                if (affectedRowCount == 0)
                {
                    ModelState.AddModelError("", "adding is unsuccessfull");//sol tarafa property ismi girilirse hata bu girilen property ile ilişkilendirilir
                    //bu alan boş bırakılırsa summary alanında genel bir hata olarak görüntülenir
                }
                else
                {
                    return RedirectToAction(nameof(Login));
                }
            }
            return View(model);
        }

        public IActionResult Profile()
        {
            return View();
        }
    }
}
