using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DatabaseProject.Filters
{
    /// <summary>
    /// Session kontrolü yapan Action Filter
    /// Kullanıcı giriş yapmamışsa Login sayfasına yönlendirir
    /// </summary>
    public class SessionCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            
            // Session'da UserID yoksa giriş yapılmamış demektir
            if (session.GetInt32("UserID") == null)
            {
                // ReturnUrl'i sakla, giriş yaptıktan sonra buraya dönebilir
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    session.SetString("ReturnUrl", returnUrl);
                }
                
                // Login sayfasına yönlendir
                context.Result = new RedirectToActionResult("Login", "Account", null);
                
                // TempData ile mesaj gönder
                context.HttpContext.Items["ErrorMessage"] = "Lütfen giriş yapın.";
            }
            
            base.OnActionExecuting(context);
        }
    }
}
