using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DatabaseProject.DAL;
using DatabaseProject.Models;

namespace DatabaseProject.Controllers
{
    public class TransactionController : Controller
    {
        private readonly TransactionDAL _transactionDAL;
        private readonly CustomerDAL _customerDAL; // Müşteri listesi için lazım

        public TransactionController(IConfiguration configuration)
        {
            _transactionDAL = new TransactionDAL(configuration);
            _customerDAL = new CustomerDAL(configuration);
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
                // UserID şimdilik 1 (Admin) sabit, sonra Session'dan çekeriz
                int currentUserId = 1; 

                _transactionDAL.AddTransaction(CustomerID, MethodID, AccountID, Amount, Description, currentUserId);
                
                TempData["SuccessMessage"] = "İşlem başarıyla kaydedildi ve bakiye güncellendi.";
                return RedirectToAction("Index", "Customer"); // Müşteri listesine dön
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
    }
}
