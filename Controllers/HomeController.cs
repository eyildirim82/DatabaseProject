using System.Diagnostics;
using DatabaseProject.Models;
using DatabaseProject.DAL;
using Microsoft.AspNetCore.Mvc;

namespace DatabaseProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly DashboardDAL _dashboardDAL;

        public HomeController(IConfiguration configuration)
        {
            _dashboardDAL = new DashboardDAL(configuration);
        }

        public IActionResult Index()
        {
            return View();
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
