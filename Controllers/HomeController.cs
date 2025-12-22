using System.Diagnostics;
using DatabaseProject.Models;
using DatabaseProject.DAL;
using DatabaseProject.Filters;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseProject.Controllers
{
    [SessionCheck]
    public class HomeController : Controller
    {
        private readonly DashboardDAL _dashboardDAL;

        public HomeController(IConfiguration configuration)
        {
            _dashboardDAL = new DashboardDAL(configuration);
        }

        public IActionResult Index()
        {
            try
            {
                var stats = _dashboardDAL.GetDashboardStats();
                var overdueChequesCount = _dashboardDAL.GetOverdueChequesCount();
                
                ViewBag.OverdueChequesCount = overdueChequesCount;
                return View(stats);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ana sayfa yüklenirken hata oluştu: {ex.Message}";
                return View(new DashboardStats());
            }
        }

        public IActionResult Dashboard()
        {
            try
            {
                var viewModel = new DashboardViewModel
                {
                    Stats = _dashboardDAL.GetDashboardStats(),
                    RiskStatusDistribution = _dashboardDAL.GetRiskStatusDistribution(),
                    DailyCashFlow = _dashboardDAL.GetDailyCashFlow(30),
                    CustomerRiskStatuses = _dashboardDAL.GetCustomerRiskStatuses()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Dashboard yüklenirken hata oluştu: {ex.Message}";
                return View(new DashboardViewModel());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
