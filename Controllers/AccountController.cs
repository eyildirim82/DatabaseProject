using Microsoft.AspNetCore.Mvc;
using CSE3055Project.DAL;
using CSE3055Project.Models;

namespace CSE3055Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthDAL _authDAL;

        public AccountController(IConfiguration configuration)
        {
            _authDAL = new AuthDAL(configuration);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserLoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _authDAL.LoginUser(model.Username, model.Password);

                if (user != null)
                {
                    // Session İşlemleri
                    HttpContext.Session.SetInt32("UserID", user.Value.UserId);
                    HttpContext.Session.SetString("FullName", user.Value.FullName);
                    HttpContext.Session.SetInt32("RoleID", user.Value.RoleId);

                    return RedirectToAction("Index", "Home"); // Başarılıysa anasayfaya
                }
                else
                {
                    ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
                }
            }
            return View(model);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}