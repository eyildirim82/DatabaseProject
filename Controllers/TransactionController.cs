using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DatabaseProject.DAL;
using DatabaseProject.Models;
using DatabaseProject.Filters;

namespace DatabaseProject.Controllers
{
    [SessionCheck]
    [RoleCheck(1, 2)] // Admin ve Accountant
    public class TransactionController : Controller
    {
        private readonly TransactionDAL _transactionDAL;
        private readonly CustomerDAL _customerDAL; // Müşteri listesi için lazım

        public TransactionController(IConfiguration configuration)
        {
            _transactionDAL = new TransactionDAL(configuration);
            _customerDAL = new CustomerDAL(configuration);
        }

        // GET: Transaction
        public IActionResult Index(int? customerId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var transactions = _transactionDAL.GetAllTransactions(customerId, startDate, endDate);
                var customers = _customerDAL.GetAllCustomers();

                ViewBag.Customers = new SelectList(customers, "CustomerID", "CompanyName", customerId);
                ViewBag.CustomerId = customerId;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;

                return View(transactions);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Transaction'lar yüklenirken hata oluştu: {ex.Message}";
                return View(new List<Transaction>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int CustomerID, int MethodID, int AccountID, decimal Amount, string Description)
        {
            try
            {
                // UserID'yi Session'dan al
                int? sessionUserId = HttpContext.Session.GetInt32("UserID");
                if (sessionUserId == null)
                {
                    TempData["ErrorMessage"] = "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.";
                    LoadDropdowns();
                    return View();
                }
                int currentUserId = sessionUserId.Value;

                _transactionDAL.AddTransaction(CustomerID, MethodID, AccountID, Amount, Description, currentUserId);
                
                TempData["SuccessMessage"] = "İşlem başarıyla kaydedildi ve bakiye güncellendi.";
                return RedirectToAction(nameof(Index)); // Transaction listesine dön
            }
            catch (Exception ex)
            {
                // Risk limiti hatası burada yakalanıp ekrana basılacak
                TempData["ErrorMessage"] = ex.Message;
                LoadDropdowns(); // Hata alınca dropdownlar boş gelmesin
                return View();
            }
        }

        private void LoadDropdowns()
        {
            // Müşteriler
            var customers = _customerDAL.GetAllCustomers();
            ViewBag.Customers = new SelectList(customers, "CustomerID", "CompanyName");

            // Ödeme Yöntemleri (DataTable'dan çeviri)
            var methodsDt = _transactionDAL.GetPaymentMethods();
            var methodsList = new List<SelectListItem>();
            foreach (System.Data.DataRow row in methodsDt.Rows)
            {
                methodsList.Add(new SelectListItem { Value = row["MethodID"].ToString(), Text = row["MethodName"].ToString() });
            }
            ViewBag.PaymentMethods = methodsList;

            // Hesaplar
            var accountsDt = _transactionDAL.GetCompanyAccounts();
            var accountsList = new List<SelectListItem>();
            foreach (System.Data.DataRow row in accountsDt.Rows)
            {
                accountsList.Add(new SelectListItem { Value = row["AccountID"].ToString(), Text = row["BankName"].ToString() });
            }
            ViewBag.CompanyAccounts = accountsList;
        }

        // POST: Transaction/Reverse/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reverse(int id)
        {
            try
            {
                // UserID'yi Session'dan al
                int? sessionUserId = HttpContext.Session.GetInt32("UserID");
                if (sessionUserId == null)
                {
                    TempData["ErrorMessage"] = "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.";
                    return RedirectToAction(nameof(Index));
                }

                _transactionDAL.ReverseTransaction(id, sessionUserId.Value);
                
                TempData["SuccessMessage"] = "Transaction başarıyla iptal edildi. Tersine çevrilmiş transaction oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Transaction iptal edilirken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
