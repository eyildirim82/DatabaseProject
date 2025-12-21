using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DatabaseProject.DAL;
using DatabaseProject.Models;
using DatabaseProject.Filters;

namespace DatabaseProject.Controllers
{
    [SessionCheck]
    [RoleCheck(1, 2, 3)] // Tüm roller - Admin, Accountant, Sales Rep
    public class CollectionNoteController : Controller
    {
        private readonly CollectionNoteDAL _collectionNoteDAL;
        private readonly CustomerDAL _customerDAL;

        public CollectionNoteController(IConfiguration configuration)
        {
            _collectionNoteDAL = new CollectionNoteDAL(configuration);
            _customerDAL = new CustomerDAL(configuration);
        }

        // GET: CollectionNote
        public IActionResult Index(int? customerId)
        {
            try
            {
                var notes = _collectionNoteDAL.GetAllCollectionNotes(customerId);
                var customers = _customerDAL.GetAllCustomers();

                var viewModel = new CollectionNoteViewModel
                {
                    CollectionNotes = notes,
                    CustomerID = customerId,
                    CustomerName = customerId.HasValue 
                        ? customers.FirstOrDefault(c => c.CustomerID == customerId.Value)?.CompanyName 
                        : null,
                    AvailableCustomers = customers
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Notlar yüklenirken hata oluştu: {ex.Message}";
                return View(new CollectionNoteViewModel());
            }
        }

        // GET: CollectionNote/Create
        public IActionResult Create(int customerId)
        {
            var customer = _customerDAL.GetAllCustomers().FirstOrDefault(c => c.CustomerID == customerId);
            if (customer == null)
            {
                TempData["ErrorMessage"] = "Müşteri bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CustomerID = customerId;
            ViewBag.CustomerName = customer.CompanyName;
            return View();
        }

        // POST: CollectionNote/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int customerId, string noteText, DateTime? promiseDate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(noteText))
                {
                    TempData["ErrorMessage"] = "Not metni boş olamaz.";
                    ViewBag.CustomerID = customerId;
                    var customer = _customerDAL.GetAllCustomers().FirstOrDefault(c => c.CustomerID == customerId);
                    ViewBag.CustomerName = customer?.CompanyName;
                    return View();
                }

                // UserID'yi Session'dan al
                int? sessionUserId = HttpContext.Session.GetInt32("UserID");
                if (sessionUserId == null)
                {
                    TempData["ErrorMessage"] = "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.";
                    return RedirectToAction(nameof(Index));
                }

                _collectionNoteDAL.AddCollectionNote(customerId, sessionUserId.Value, noteText, promiseDate);
                
                TempData["SuccessMessage"] = "Not başarıyla eklendi.";
                return RedirectToAction(nameof(Index), new { customerId = customerId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Not eklenirken hata oluştu: {ex.Message}";
                ViewBag.CustomerID = customerId;
                var customer = _customerDAL.GetAllCustomers().FirstOrDefault(c => c.CustomerID == customerId);
                ViewBag.CustomerName = customer?.CompanyName;
                return View();
            }
        }

        // GET: CollectionNote/Edit/5
        public IActionResult Edit(int id)
        {
            var note = _collectionNoteDAL.GetCollectionNoteById(id);
            
            if (note == null)
            {
                TempData["ErrorMessage"] = "Not bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Sadece notu yazan kullanıcı veya Admin düzenleyebilir
            int? sessionUserId = HttpContext.Session.GetInt32("UserID");
            int? roleId = HttpContext.Session.GetInt32("RoleID");
            
            if (sessionUserId == null || (sessionUserId.Value != note.UserID && roleId != 1))
            {
                TempData["ErrorMessage"] = "Bu notu düzenleme yetkiniz bulunmamaktadır.";
                return RedirectToAction(nameof(Index));
            }

            return View(note);
        }

        // POST: CollectionNote/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int noteId, string noteText, DateTime? promiseDate)
        {
            try
            {
                var note = _collectionNoteDAL.GetCollectionNoteById(noteId);
                
                if (note == null)
                {
                    TempData["ErrorMessage"] = "Not bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Sadece notu yazan kullanıcı veya Admin düzenleyebilir
                int? sessionUserId = HttpContext.Session.GetInt32("UserID");
                int? roleId = HttpContext.Session.GetInt32("RoleID");
                
                if (sessionUserId == null || (sessionUserId.Value != note.UserID && roleId != 1))
                {
                    TempData["ErrorMessage"] = "Bu notu düzenleme yetkiniz bulunmamaktadır.";
                    return RedirectToAction(nameof(Index));
                }

                if (string.IsNullOrWhiteSpace(noteText))
                {
                    TempData["ErrorMessage"] = "Not metni boş olamaz.";
                    return View(note);
                }

                _collectionNoteDAL.UpdateCollectionNote(noteId, noteText, promiseDate);
                
                TempData["SuccessMessage"] = "Not başarıyla güncellendi.";
                return RedirectToAction(nameof(Index), new { customerId = note.CustomerID });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Not güncellenirken hata oluştu: {ex.Message}";
                var note = _collectionNoteDAL.GetCollectionNoteById(noteId);
                return View(note);
            }
        }

        // POST: CollectionNote/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var note = _collectionNoteDAL.GetCollectionNoteById(id);
                
                if (note == null)
                {
                    TempData["ErrorMessage"] = "Not bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Sadece notu yazan kullanıcı veya Admin silebilir
                int? sessionUserId = HttpContext.Session.GetInt32("UserID");
                int? roleId = HttpContext.Session.GetInt32("RoleID");
                
                if (sessionUserId == null || (sessionUserId.Value != note.UserID && roleId != 1))
                {
                    TempData["ErrorMessage"] = "Bu notu silme yetkiniz bulunmamaktadır.";
                    return RedirectToAction(nameof(Index));
                }

                int customerId = note.CustomerID;
                _collectionNoteDAL.DeleteCollectionNote(id);
                
                TempData["SuccessMessage"] = "Not başarıyla silindi.";
                return RedirectToAction(nameof(Index), new { customerId = customerId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Not silinirken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
