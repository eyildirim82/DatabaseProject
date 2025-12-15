using Microsoft.AspNetCore.Mvc;
using DatabaseProject.DAL;
using DatabaseProject.Models;
using ClosedXML.Excel;

namespace DatabaseProject.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CustomerDAL _customerDAL;

        public CustomerController(IConfiguration configuration)
        {
            _customerDAL = new CustomerDAL(configuration);
        }

        public IActionResult Index()
        {
            var customers = _customerDAL.GetAllCustomers();
            return View(customers);
        }

        [HttpGet]
        public IActionResult ImportExcel()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
                return View();
            }

            var allowedExtensions = new[] { ".txt", ".xlsx", ".xls", ".csv" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Sadece .txt, .xlsx, .xls ve .csv dosyaları yüklenebilir.";
                return View();
            }

            try
            {
                string fileContent;
                
                if (extension == ".xlsx" || extension == ".xls")
                {
                    fileContent = ReadExcelAsText(file);
                }
                else
                {
                    using (var reader = new StreamReader(file.OpenReadStream(), System.Text.Encoding.UTF8, true))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                }

                if (string.IsNullOrWhiteSpace(fileContent) || fileContent.Length < 50)
                {
                    TempData["ErrorMessage"] = "Dosya boş veya geçersiz format.";
                    return View();
                }

                var result = _customerDAL.ParseAndImportReport(fileContent);

                if (result.SuccessCount > 0)
                {
                    TempData["SuccessMessage"] = $"{result.SuccessCount} müşteri başarıyla aktarıldı!";
                }
                if (result.ErrorCount > 0)
                {
                    TempData["ErrorMessage"] = $"{result.ErrorCount} blokta hata oluştu.";
                    TempData["ImportErrors"] = result.Errors.Take(10).ToList();
                }
                if (result.SuccessCount == 0 && result.ErrorCount == 0)
                {
                    TempData["ErrorMessage"] = "Dosyada işlenebilir müşteri verisi bulunamadı.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Pipeline hatası: {ex.Message}";
                return View();
            }

            return RedirectToAction(nameof(Index));
        }

        private string ReadExcelAsText(IFormFile file)
        {
            var sb = new System.Text.StringBuilder();

            using (var stream = file.OpenReadStream())
            using (var workbook = new XLWorkbook(stream))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    var usedRange = worksheet.RangeUsed();
                    if (usedRange == null) continue;

                    foreach (var row in usedRange.Rows())
                    {
                        var rowText = new List<string>();
                        foreach (var cell in row.Cells())
                        {
                            var value = cell.GetFormattedString();
                            if (!string.IsNullOrWhiteSpace(value))
                            {
                                rowText.Add(value);
                            }
                        }
                        if (rowText.Count > 0)
                        {
                            sb.AppendLine(string.Join(" ", rowText));
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }
}
