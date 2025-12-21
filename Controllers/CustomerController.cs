using Microsoft.AspNetCore.Mvc;
using DatabaseProject.DAL;
using DatabaseProject.Models;
using ClosedXML.Excel;
using System.Linq;

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
        public IActionResult ImportExcel(string? statusFilter, string? fileNameFilter, 
            DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 10, 
            string sortBy = "UploadDate", string sortDirection = "DESC")
        {
            var viewModel = new ImportBatchViewModel
            {
                StatusFilter = statusFilter,
                FileNameFilter = fileNameFilter,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            int totalRecords;
            viewModel.Batches = _customerDAL.GetImportHistory(
                statusFilter, fileNameFilter, startDate, endDate,
                page, pageSize, sortBy, sortDirection, out totalRecords);
            
            viewModel.TotalRecords = totalRecords;
            viewModel.Statistics = _customerDAL.GetBatchStatistics();

            return View(viewModel);
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

        [HttpGet]
        public IActionResult BatchDetails(int id)
        {
            var batch = _customerDAL.GetImportHistory()
                .FirstOrDefault(b => b.BatchID == id);
            
            if (batch == null)
            {
                TempData["ErrorMessage"] = "Batch bulunamadı.";
                return RedirectToAction(nameof(ImportExcel));
            }

            var details = _customerDAL.GetBatchDetails(id);
            ViewBag.Batch = batch;
            return View(details);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateBatchStatus(int batchId, string status)
        {
            try
            {
                bool success = _customerDAL.UpdateBatchStatus(batchId, status);
                if (success)
                {
                    TempData["SuccessMessage"] = "Batch durumu başarıyla güncellendi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Batch durumu güncellenemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction(nameof(ImportExcel));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBatch(int batchId)
        {
            try
            {
                bool success = _customerDAL.DeleteBatch(batchId);
                if (success)
                {
                    TempData["SuccessMessage"] = "Batch başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Batch silinemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }

            return RedirectToAction(nameof(ImportExcel));
        }

        [HttpGet]
        public IActionResult ExportHistory(string format = "csv", string? statusFilter = null, 
            string? fileNameFilter = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            int totalRecords;
            var batches = _customerDAL.GetImportHistory(
                statusFilter, fileNameFilter, startDate, endDate,
                1, 10000, "UploadDate", "DESC", out totalRecords);

            if (format.ToLower() == "excel")
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Import History");
                    
                    // Başlıklar
                    worksheet.Cell(1, 1).Value = "Batch ID";
                    worksheet.Cell(1, 2).Value = "Dosya Adı";
                    worksheet.Cell(1, 3).Value = "Dosya Tarihi";
                    worksheet.Cell(1, 4).Value = "Yükleme Tarihi";
                    worksheet.Cell(1, 5).Value = "Kayıt Sayısı";
                    worksheet.Cell(1, 6).Value = "Durum";

                    // Stil
                    var headerRange = worksheet.Range(1, 1, 1, 6);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Veriler
                    int row = 2;
                    foreach (var batch in batches)
                    {
                        worksheet.Cell(row, 1).Value = batch.BatchID;
                        worksheet.Cell(row, 2).Value = batch.FileName;
                        worksheet.Cell(row, 3).Value = batch.FileTimestamp;
                        worksheet.Cell(row, 4).Value = batch.UploadDate;
                        worksheet.Cell(row, 5).Value = batch.TotalRecords;
                        worksheet.Cell(row, 6).Value = batch.Status;
                        row++;
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, 
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"ImportHistory_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                    }
                }
            }
            else // CSV
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Batch ID,Dosya Adı,Dosya Tarihi,Yükleme Tarihi,Kayıt Sayısı,Durum");
                
                foreach (var batch in batches)
                {
                    csv.AppendLine($"{batch.BatchID},\"{batch.FileName}\",{batch.FileTimestamp:yyyy-MM-dd HH:mm:ss},{batch.UploadDate:yyyy-MM-dd HH:mm:ss},{batch.TotalRecords},{batch.Status}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"ImportHistory_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
        }
    }
}
