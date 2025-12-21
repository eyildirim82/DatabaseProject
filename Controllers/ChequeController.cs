using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DatabaseProject.DAL;
using DatabaseProject.Models;

namespace DatabaseProject.Controllers
{
    public class ChequeController : Controller
    {
        private readonly ChequeDAL _chequeDAL;
        private readonly CustomerDAL _customerDAL;

        public ChequeController(IConfiguration configuration)
        {
            _chequeDAL = new ChequeDAL(configuration);
            _customerDAL = new CustomerDAL(configuration);
        }

        // GET: Cheque
        public IActionResult Index(string? statusFilter)
        {
            try
            {
                var cheques = _chequeDAL.GetAllCheques(statusFilter);
                
                var viewModel = new ChequeViewModel
                {
                    Cheques = cheques,
                    StatusFilter = statusFilter
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Çekler yüklenirken hata oluştu: {ex.Message}";
                return View(new ChequeViewModel());
            }
        }

        // GET: Cheque/Create
        public IActionResult Create()
        {
            LoadDropdowns();
            return View();
        }

        // POST: Cheque/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int CustomerID, string BankName, decimal Amount, DateTime DueDate, string? ChequeNumber)
        {
            try
            {
                if (Amount <= 0)
                {
                    TempData["ErrorMessage"] = "Tutar pozitif bir değer olmalıdır.";
                    LoadDropdowns();
                    return View();
                }

                if (DueDate < DateTime.Today)
                {
                    TempData["ErrorMessage"] = "Vade tarihi bugünden önce olamaz.";
                    LoadDropdowns();
                    return View();
                }

                _chequeDAL.AddCheque(CustomerID, BankName, Amount, DueDate, ChequeNumber);
                
                TempData["SuccessMessage"] = "Çek başarıyla eklendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                LoadDropdowns();
                return View();
            }
        }

        // GET: Cheque/Edit/5
        public IActionResult Edit(int id)
        {
            var cheque = _chequeDAL.GetChequeById(id);
            
            if (cheque == null)
            {
                TempData["ErrorMessage"] = "Çek bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AvailableStatuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Portfolio", Text = "Portfolio" },
                new SelectListItem { Value = "Collected", Text = "Collected" },
                new SelectListItem { Value = "Bounced", Text = "Bounced" },
                new SelectListItem { Value = "Returned", Text = "Returned" }
            };

            return View(cheque);
        }

        // POST: Cheque/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int ChequeID, string Status)
        {
            try
            {
                var cheque = _chequeDAL.GetChequeById(ChequeID);
                
                if (cheque == null)
                {
                    TempData["ErrorMessage"] = "Çek bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                _chequeDAL.UpdateChequeStatus(ChequeID, Status);
                
                TempData["SuccessMessage"] = "Çek durumu başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Durum güncellenirken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id = ChequeID });
            }
        }

        // POST: Cheque/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var cheque = _chequeDAL.GetChequeById(id);
                
                if (cheque == null)
                {
                    TempData["ErrorMessage"] = "Çek bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                _chequeDAL.DeleteCheque(id);
                
                TempData["SuccessMessage"] = "Çek başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Çek silinirken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        private void LoadDropdowns()
        {
            var customers = _customerDAL.GetAllCustomers();
            ViewBag.Customers = new SelectList(customers, "CustomerID", "CompanyName");
        }
    }
}
