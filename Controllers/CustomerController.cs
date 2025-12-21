using Microsoft.AspNetCore.Mvc;
using DatabaseProject.DAL;
using DatabaseProject.Models;
using ClosedXML.Excel;
using System.Linq;
using Microsoft.AspNetCore.Http;

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
            try
            {
                // İstatistikleri al
                var statistics = _customerDAL.GetBatchStatistics();

                // Filtrelenmiş ve sayfalanmış verileri al
                int totalRecords;
                var batches = _customerDAL.GetImportHistoryWithFilters(
                    statusFilter, fileNameFilter, startDate, endDate,
                    page, pageSize, sortBy, sortDirection, out totalRecords);

                // ViewModel oluştur
                var viewModel = new ImportBatchViewModel
                {
                    Batches = batches,
                    StatusFilter = statusFilter,
                    FileNameFilter = fileNameFilter,
                    StartDate = startDate,
                    EndDate = endDate,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    SortBy = sortBy,
                    SortDirection = sortDirection,
                    Statistics = statistics
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
                return View(new ImportBatchViewModel 
                { 
                    Batches = new List<ImportBatch>(),
                    Statistics = _customerDAL.GetBatchStatistics()
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Lütfen bir dosya seçin.";
                return RedirectToAction(nameof(ImportExcel));
            }

            var allowedExtensions = new[] { ".txt", ".xlsx", ".xls", ".csv" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] = "Sadece .txt, .xlsx, .xls ve .csv dosyaları yüklenebilir.";
                return RedirectToAction(nameof(ImportExcel));
            }

            // Dosya isminden tarih parse etme
            // Beklenen Format: TopluCariEkstreRaporu_yyyyMMddHHmmss.xlsx
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
                    TempData["ErrorMessage"] = "Dosya isminde geçerli bir tarih formatı bulunamadı (Örn: TopluCariEkstreRaporu_20251215142935.xlsx).";
                    return RedirectToAction(nameof(ImportExcel));
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Dosya isminde geçerli bir tarih formatı bulunamadı (Örn: _20251215142935).";
                return RedirectToAction(nameof(ImportExcel));
            }

            try
            {
                // Excel dosyasını oku ve satır sayısını hesapla
                int totalRecords = 0;
                
                if (extension == ".xlsx" || extension == ".xls")
                {
                    using (var stream = file.OpenReadStream())
                    using (var workbook = new XLWorkbook(stream))
                    {
                        foreach (var worksheet in workbook.Worksheets)
                        {
                            var usedRange = worksheet.RangeUsed();
                            if (usedRange != null)
                            {
                                totalRecords = usedRange.RowCount();
                            }
                        }
                    }
                }
                else
                {
                    // Text dosyaları için satır sayısını hesapla
                    using (var reader = new StreamReader(file.OpenReadStream(), System.Text.Encoding.UTF8, true))
                    {
                        while (await reader.ReadLineAsync() != null)
                        {
                            totalRecords++;
                        }
                    }
                }

                // UserID'yi Session'dan al (Session yoksa veya UserID yoksa default 1 kullan)
                int userId = 1; // Default Admin user ID
                if (HttpContext.Session != null && HttpContext.Session.IsAvailable)
                {
                    userId = HttpContext.Session.GetInt32("UserID") ?? 1;
                }

                // DAL'daki CreateImportBatch metodunu çağır
                int batchId = await _customerDAL.CreateImportBatchAsync(fileName, fileTimestamp, userId, totalRecords);

                TempData["SuccessMessage"] = $"Dosya başarıyla yüklendi! Batch ID: {batchId}, Toplam Kayıt: {totalRecords}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"İşlem Başarısız: {ex.Message}";
            }

            return RedirectToAction(nameof(ImportExcel));
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
            var batch = _customerDAL.GetImportHistoryWithFilters()
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
            var batches = _customerDAL.GetImportHistoryWithFilters(
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

        [HttpGet]
        public IActionResult ExportBatchDetails(int id, string format = "excel")
        {
            var batch = _customerDAL.GetImportHistoryWithFilters()
                .FirstOrDefault(b => b.BatchID == id);
            
            if (batch == null)
            {
                TempData["ErrorMessage"] = "Batch bulunamadı.";
                return RedirectToAction(nameof(ImportExcel));
            }

            var details = _customerDAL.GetBatchDetails(id);

            if (format.ToLower() == "excel")
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add($"Batch {id} Detayları");
                    
                    // Batch bilgileri
                    worksheet.Cell(1, 1).Value = "Batch ID:";
                    worksheet.Cell(1, 2).Value = batch.BatchID;
                    worksheet.Cell(2, 1).Value = "Dosya Adı:";
                    worksheet.Cell(2, 2).Value = batch.FileName;
                    worksheet.Cell(3, 1).Value = "Dosya Tarihi:";
                    worksheet.Cell(3, 2).Value = batch.FileTimestamp;
                    worksheet.Cell(4, 1).Value = "Yükleme Tarihi:";
                    worksheet.Cell(4, 2).Value = batch.UploadDate;
                    worksheet.Cell(5, 1).Value = "Durum:";
                    worksheet.Cell(5, 2).Value = batch.Status;

                    // Detay başlıkları
                    worksheet.Cell(7, 1).Value = "Hesap Kodu";
                    worksheet.Cell(7, 2).Value = "Tespit Edilen İsim";
                    worksheet.Cell(7, 3).Value = "Excel Bakiyesi";
                    worksheet.Cell(7, 4).Value = "Sistem Bakiyesi";
                    worksheet.Cell(7, 5).Value = "Fark";

                    // Stil
                    var headerRange = worksheet.Range(7, 1, 7, 5);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Veriler
                    int row = 8;
                    foreach (var detail in details)
                    {
                        worksheet.Cell(row, 1).Value = detail.AccountCode;
                        worksheet.Cell(row, 2).Value = detail.DetectedName;
                        worksheet.Cell(row, 3).Value = detail.ExcelBalance;
                        worksheet.Cell(row, 4).Value = detail.SystemBalanceAtTime;
                        worksheet.Cell(row, 5).Value = detail.BalanceDifference;
                        row++;
                    }

                    // Toplam satırı
                    worksheet.Cell(row, 1).Value = "TOPLAM";
                    worksheet.Cell(row, 3).Value = details.Sum(d => d.ExcelBalance);
                    worksheet.Cell(row, 4).Value = details.Sum(d => d.SystemBalanceAtTime);
                    worksheet.Cell(row, 5).Value = details.Sum(d => d.BalanceDifference);
                    var totalRange = worksheet.Range(row, 1, row, 5);
                    totalRange.Style.Font.Bold = true;
                    totalRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, 
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Batch_{id}_Detaylar_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                    }
                }
            }
            else // CSV
            {
                var csv = new System.Text.StringBuilder();
                csv.AppendLine($"Batch ID,{batch.BatchID}");
                csv.AppendLine($"Dosya Adı,\"{batch.FileName}\"");
                csv.AppendLine($"Dosya Tarihi,{batch.FileTimestamp:yyyy-MM-dd HH:mm:ss}");
                csv.AppendLine($"Yükleme Tarihi,{batch.UploadDate:yyyy-MM-dd HH:mm:ss}");
                csv.AppendLine($"Durum,{batch.Status}");
                csv.AppendLine();
                csv.AppendLine("Hesap Kodu,Tespit Edilen İsim,Excel Bakiyesi,Sistem Bakiyesi,Fark");
                
                foreach (var detail in details)
                {
                    csv.AppendLine($"{detail.AccountCode},\"{detail.DetectedName}\",{detail.ExcelBalance},{detail.SystemBalanceAtTime},{detail.BalanceDifference}");
                }

                csv.AppendLine();
                csv.AppendLine($"TOPLAM,,{details.Sum(d => d.ExcelBalance)},{details.Sum(d => d.SystemBalanceAtTime)},{details.Sum(d => d.BalanceDifference)}");

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"Batch_{id}_Detaylar_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
        }
    }
}
