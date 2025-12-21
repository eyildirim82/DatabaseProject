using Microsoft.AspNetCore.Mvc;
using DatabaseProject.DAL;
using DatabaseProject.Models;

namespace DatabaseProject.Controllers
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

        [HttpGet]
        public IActionResult ChangePassword()
        {
            // Session kontrolü
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                TempData["ErrorMessage"] = "Lütfen giriş yapın.";
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            // Session kontrolü
            int? sessionUserId = HttpContext.Session.GetInt32("UserID");
            if (sessionUserId == null)
            {
                TempData["ErrorMessage"] = "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login");
            }

            // Validasyonlar
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                ViewBag.ErrorMessage = "Mevcut şifre boş olamaz.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ViewBag.ErrorMessage = "Yeni şifre boş olamaz.";
                return View();
            }

            if (newPassword.Length < 6)
            {
                ViewBag.ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "Yeni şifre ve şifre tekrarı eşleşmiyor.";
                return View();
            }

            if (currentPassword == newPassword)
            {
                ViewBag.ErrorMessage = "Yeni şifre mevcut şifre ile aynı olamaz.";
                return View();
            }

            try
            {
                bool success = _authDAL.ChangePassword(sessionUserId.Value, currentPassword, newPassword);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.ErrorMessage = "Şifre değiştirilemedi.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }
    }
}