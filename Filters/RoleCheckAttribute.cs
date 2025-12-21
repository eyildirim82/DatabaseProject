using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DatabaseProject.Filters
{
    /// <summary>
    /// Role-based yetkilendirme kontrolü yapan Action Filter
    /// Belirtilen rol ID'lerine sahip kullanıcıların erişimine izin verir
    /// </summary>
    public class RoleCheckAttribute : ActionFilterAttribute
    {
        private readonly int[] _allowedRoleIds;
        
        /// <summary>
        /// RoleCheck attribute constructor
        /// </summary>
        /// <param name="allowedRoleIds">Erişime izin verilen rol ID'leri (örn: 1 = Admin, 2 = Accountant, 3 = Sales Rep)</param>
        public RoleCheckAttribute(params int[] allowedRoleIds)
        {
            _allowedRoleIds = allowedRoleIds ?? Array.Empty<int>();
        }
        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            
            // Önce session kontrolü
            if (session.GetInt32("UserID") == null)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                context.HttpContext.Items["ErrorMessage"] = "Lütfen giriş yapın.";
                base.OnActionExecuting(context);
                return;
            }
            
            // RoleID kontrolü
            var roleId = session.GetInt32("RoleID");
            if (roleId == null || !_allowedRoleIds.Contains(roleId.Value))
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
                context.HttpContext.Items["ErrorMessage"] = "Bu sayfaya erişim yetkiniz bulunmamaktadır.";
                return;
            }
            
            base.OnActionExecuting(context);
        }
    }
}
