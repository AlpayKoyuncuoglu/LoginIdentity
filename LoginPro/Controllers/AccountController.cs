using LoginPro.Entities;
using LoginPro.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using NETCore.Encrypt.Extensions;
using System.ComponentModel.DataAnnotations;
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
                string hashedPassword = DoMD5HashedString(model.Password);


                //firstOrDefault kullanılmadı, birden çok varsa hata döndürmesi istenildi
                //aslında aynı username ile ekleme engellenmiş olmasına rağment burada ekstra bir kontrol yapıldı
                User user = _databaseContext.Users.SingleOrDefault(u => u.Username.ToLower() == model.Username.ToLower() && u.Password == hashedPassword);
                if (user != null)
                {
                    if (user.Locked)
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
                    claims.Add(new Claim(ClaimTypes.Role, user.Role));
                    claims.Add(new Claim("Username", user.Username));
                    //ClaimsIdentity identity = new ClaimsIdentity(claims,"Cookies");//aşağıdaki gibi düzenlendi
                    ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "username or password incorrect");
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
                string hashedPassword = DoMD5HashedString(model.Password);

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
            //Guid userid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));//FindFirst kullanılasaydı sonuna .value denmesi gerekirdi
            //User user = _databaseContext.Users.SingleOrDefault(x => x.Id == userid);

            ProfileInfoLoader();

            return View();
        }

        private void ProfileInfoLoader()
        {
            Guid userid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            User user = _databaseContext.Users.SingleOrDefault(x => x.Id == userid);

            ViewData["FullName"] = user.FullName; 
        }


        [HttpPost]
        public IActionResult ProfileChangeFullName([Required][StringLength(50)] string? fullname)//buraya gelen değer html tarafında input tag'i içinde name ile belirlendi
        {
            if (ModelState.IsValid)
            { 
                Guid userid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));//FindFirst kullanılasaydı sonuna .value denmesi gerekirdi
                User user = _databaseContext.Users.SingleOrDefault(x => x.Id == userid);

                user.FullName = fullname;
                _databaseContext.SaveChanges();

                return RedirectToAction(nameof(Profile));
            }

            ProfileInfoLoader();
            return View("Profile");//hatanın gözükmesi için redirectToAction kullanılmadı
        }

        [HttpPost]
        public IActionResult ProfileChangePassword([Required][MinLength(6)][MaxLength(16)] string? password)
        {
            if (ModelState.IsValid)
            {
                Guid userid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                User user = _databaseContext.Users.SingleOrDefault(x => x.Id == userid);

                string hashedPassword = DoMD5HashedString(password);

                user.Password = hashedPassword;
                _databaseContext.SaveChanges();

                ViewData["result"] = "PasswordChanged";
            }

            ProfileInfoLoader();
            return View("Profile");
        }
        private string DoMD5HashedString(string s)
        {
            string md5Salt = _configuration.GetValue<string>("AppSettings:MD5Salt");
            string salted = s + md5Salt;
            string hashed = salted.MD5();
            return hashed;
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }
    }
}
