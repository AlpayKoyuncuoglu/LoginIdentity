using LoginPro.Entities;
using LoginPro.Models;
using Microsoft.AspNetCore.Mvc;

namespace LoginPro.Controllers
{
    public class AccountController : Controller
    {
        private readonly DatabaseContext _databaseContext;

        public AccountController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
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
                //login işlemleri
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
                User user = new()
                {
                    Username = model.Username,
                    Password = model.Password,
                };
                _databaseContext.Users.Add(user);
                int affectedRowCount = _databaseContext.SaveChanges();//saveChanges int döner: etkilenen row sayısı
                if (affectedRowCount > 0)
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
