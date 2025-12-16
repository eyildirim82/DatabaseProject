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

            // 1. Dosya isminden tarih parse etme
            // Beklenen Format: TopluCariEkstreRaporu_20251215142935.xlsx
            string fileName = Path.GetFileName(file.FileName);
            DateTime fileTimestamp;

            try
            {
                // Regex ile sadece sayısal tarih kısmını al (dosya adı değişse bile çalışır)
                var match = System.Text.RegularExpressions.Regex.Match(fileName, @"(\d{14})");
                if (match.Success)
                {
                    string datePart = match.Groups[1].Value; // 20251215142935
                    fileTimestamp = DateTime.ParseExact(datePart, "yyyyMMddHHmmss", null);
                }
                else
                {
                    // Bilinçli olarak hata fırlatıyoruz; catch bloğu kullanıcıya mesaj gösterecek
                    throw new FormatException("Filename does not contain a valid 14-digit datetime.");
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Dosya isminde geçerli bir tarih formatı bulunamadı (Örn: _20251215142935).";
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

                // 2. DAL Çağrısı (Tarih parametresi ile)
                // UserID'yi şimdilik 1 (Admin) gönderiyoruz, Login sistemi varsa User.Identity'den alabilirsin.
                var result = _customerDAL.ParseAndImportReport(fileContent, fileName, fileTimestamp, 1);

                if (result.SuccessCount > 0 && result.ErrorCount == 0)
                {
                    TempData["SuccessMessage"] = $"{result.SuccessCount} kayıt işlendi. Mutabakat tamamlandı!";
                }
                else if (result.SuccessCount > 0 && result.ErrorCount > 0)
                {
                    TempData["ErrorMessage"] = $"{result.SuccessCount} kayıt işlendi, {result.ErrorCount} kayıt işlenemedi. Lütfen detayları kontrol edin.";
                }
                else if (result.SuccessCount == 0 && result.ErrorCount > 0)
                {
                    TempData["ErrorMessage"] = $"Tüm kayıtlar işlenemedi ({result.ErrorCount} hata). Lütfen dosya içeriğini kontrol edin.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Hiçbir kayıt işlenemedi. Lütfen dosya içeriğini kontrol edin.";
                }
            }
            catch (Exception ex)
            {
                // SQL'den gelen "Eski Dosya" hatası burada kullanıcıya gösterilecek
                TempData["ErrorMessage"] = $"İşlem Başarısız: {ex.Message}";
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
