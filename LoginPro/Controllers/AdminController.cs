using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoginPro.Controllers
{
    [Authorize(Roles = "admin")]//controller üzerinde yapılan authorization bütün actionlar için geçerlidir ancak sadece belirli actionlar için geçerli olsun isteniyorsa sadece o actionlar üzerine eklenmelidir
    public class AdminController : Controller
    {
        //[Authorize(Roles = "admin,manager")]//bu roller bu action'ı tetikleyebilir
        public IActionResult Index()
        {
            return View();
        }
    }
}
