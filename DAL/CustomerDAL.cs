using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using DatabaseProject.Models;

namespace DatabaseProject.DAL
{
    public class CustomerDAL
    {
        private readonly string _connectionString;

        public CustomerDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        public List<Customer> GetAllCustomers()
        {
            var customers = new List<Customer>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllCustomers", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            customers.Add(new Customer
                            {
                                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                                AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                                CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                                TaxID = reader.IsDBNull(reader.GetOrdinal("TaxID")) ? null : reader.GetString(reader.GetOrdinal("TaxID")),
                                TaxOffice = reader.IsDBNull(reader.GetOrdinal("TaxOffice")) ? null : reader.GetString(reader.GetOrdinal("TaxOffice")),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                RiskLimit = reader.GetDecimal(reader.GetOrdinal("RiskLimit")),
                                CurrentBalance = reader.GetDecimal(reader.GetOrdinal("CurrentBalance"))
                            });
                        }
                    }
                }
            }

            return customers;
        }

        /// <summary>
        /// Customer ID'ye göre müşteri bilgilerini getirir
        /// </summary>
        public Customer? GetCustomerById(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetCustomerById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerID", customerId);
                    conn.Open();
                    
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Customer
                            {
                                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                                AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                                CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                                TaxID = reader.IsDBNull(reader.GetOrdinal("TaxID")) ? null : reader.GetString(reader.GetOrdinal("TaxID")),
                                TaxOffice = reader.IsDBNull(reader.GetOrdinal("TaxOffice")) ? null : reader.GetString(reader.GetOrdinal("TaxOffice")),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                RiskLimit = reader.GetDecimal(reader.GetOrdinal("RiskLimit")),
                                CurrentBalance = reader.GetDecimal(reader.GetOrdinal("CurrentBalance"))
                            };
                        }
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// Yeni müşteri ekler (sp_AddCustomer stored procedure ile)
        /// </summary>
        public void AddCustomer(Customer customer)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_AddCustomer", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@AccountCode", customer.AccountCode);
                    cmd.Parameters.AddWithValue("@CompanyName", customer.CompanyName);
                    cmd.Parameters.AddWithValue("@TaxID", (object?)customer.TaxID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TaxOffice", (object?)customer.TaxOffice ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object?)customer.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhoneNumber", (object?)customer.PhoneNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RiskLimit", customer.RiskLimit);

                    conn.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Müşteri eklenirken hata oluştu: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Müşteri bilgilerini günceller (sp_UpdateCustomer stored procedure ile)
        /// </summary>
        public void UpdateCustomer(Customer customer)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateCustomer", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
                    cmd.Parameters.AddWithValue("@CompanyName", customer.CompanyName);
                    cmd.Parameters.AddWithValue("@TaxID", (object?)customer.TaxID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TaxOffice", (object?)customer.TaxOffice ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object?)customer.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PhoneNumber", (object?)customer.PhoneNumber ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RiskLimit", customer.RiskLimit);

                    conn.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Müşteri güncellenirken hata oluştu: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Müşteri siler (sp_DeleteCustomer stored procedure ile)
        /// </summary>
        public void DeleteCustomer(int customerId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteCustomer", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CustomerID", customerId);

                    conn.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Müşteri silinirken hata oluştu: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Geçmiş yüklemeleri getirir (en son 20 kayıt) - Async versiyon
        /// </summary>
        public async Task<List<ImportBatch>> GetImportHistoryAsync()
        {
            var batches = new List<ImportBatch>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                using (var cmd = new SqlCommand("sp_GetImportHistory", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TopCount", 20);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            batches.Add(new ImportBatch
                            {
                                BatchID = reader.GetInt32(reader.GetOrdinal("BatchID")),
                                FileName = reader.GetString(reader.GetOrdinal("FileName")),
                                FileTimestamp = reader.GetDateTime(reader.GetOrdinal("FileTimestamp")),
                                UploadDate = reader.GetDateTime(reader.GetOrdinal("UploadDate")),
                                TotalRecords = reader.GetInt32(reader.GetOrdinal("TotalRecords")),
                                Status = reader.GetString(reader.GetOrdinal("Status"))
                            });
                        }
                    }
                }
            }

            return batches;
        }

        /// <summary>
        /// Geçmiş yüklemeleri getirir (en son 20 kayıt) - Basit versiyon
        /// </summary>
        public async Task<List<ImportBatch>> GetImportHistory()
        {
            return await GetImportHistoryAsync();
        }

        /// <summary>
        /// ImportBatch kaydı oluşturur (sp_ValidateAndCreateBatch prosedürünü çağırır)
        /// </summary>
        /// <param name="fileName">Dosya adı</param>
        /// <param name="fileTimestamp">Dosya isminden parse edilen tarih</param>
        /// <param name="userId">Yükleyen kullanıcı ID</param>
        /// <param name="totalRecords">Toplam kayıt sayısı</param>
        /// <returns>Oluşturulan BatchID</returns>
        public async Task<int> CreateImportBatchAsync(string fileName, DateTime fileTimestamp, int userId, int totalRecords)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var cmd = new SqlCommand("sp_ValidateAndCreateBatch", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FileName", fileName);
                    cmd.Parameters.AddWithValue("@FileTimestamp", fileTimestamp);
                    cmd.Parameters.AddWithValue("@UploadedBy", userId);
                    cmd.Parameters.AddWithValue("@TotalRecords", totalRecords);

                    // Output parametresi
                    var outParam = new SqlParameter("@BatchID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outParam);

                    try
                    {
                        await cmd.ExecuteNonQueryAsync();

                        if (outParam.Value == DBNull.Value || outParam.Value == null)
                        {
                            throw new Exception("Batch oluşturulamadı, BatchID döndürülmedi.");
                        }

                        int batchId = (int)outParam.Value;

                        if (batchId <= 0)
                        {
                            throw new Exception("Geçersiz BatchID alındı (0 veya negatif). İşlem iptal edildi.");
                        }

                        return batchId;
                    }
                    catch (SqlException ex)
                    {
                        // Stored Procedure'den gelen özel hataları yakala
                        if ((ex.Number >= 50000 && ex.Number <= 50999) ||
                            ex.Message.Contains("daha güncel bir veri zaten yüklü"))
                        {
                            throw new Exception(ex.Message);
                        }
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Filtrelenmiş ve sayfalanmış geçmiş yüklemeleri getirir (eski versiyon - filtreleme için)
        /// </summary>
        public List<ImportBatch> GetImportHistoryWithFilters()
        {
            return GetImportHistoryWithFilters(null, null, null, null, 1, 10, "UploadDate", "DESC", out _);
        }

        /// <summary>
        /// Filtrelenmiş ve sayfalanmış geçmiş yüklemeleri getirir (eski versiyon - filtreleme için)
        /// </summary>
        public List<ImportBatch> GetImportHistoryWithFilters(string? statusFilter, string? fileNameFilter, 
            DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, 
            string sortBy, string sortDirection, out int totalRecords)
        {
            var batches = new List<ImportBatch>();
            totalRecords = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("sp_GetImportHistoryWithFilters", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Status", (object?)statusFilter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileName", (object?)fileNameFilter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    cmd.Parameters.AddWithValue("@SortBy", sortBy);
                    cmd.Parameters.AddWithValue("@SortDirection", sortDirection);
                    
                    var totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(totalRecordsParam);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            batches.Add(new ImportBatch
                            {
                                BatchID = reader.GetInt32(reader.GetOrdinal("BatchID")),
                                FileName = reader.GetString(reader.GetOrdinal("FileName")),
                                FileTimestamp = reader.GetDateTime(reader.GetOrdinal("FileTimestamp")),
                                UploadDate = reader.GetDateTime(reader.GetOrdinal("UploadDate")),
                                TotalRecords = reader.GetInt32(reader.GetOrdinal("TotalRecords")),
                                Status = reader.GetString(reader.GetOrdinal("Status"))
                            });
                        }
                    }
                    
                    totalRecords = totalRecordsParam.Value != DBNull.Value ? (int)totalRecordsParam.Value : 0;
                }
            }

            return batches;
        }

        public List<ImportDetail> GetBatchDetails(int batchId)
        {
            var details = new List<ImportDetail>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetBatchDetails", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BatchID", batchId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            details.Add(new ImportDetail
                            {
                                DetailID = reader.GetInt32(reader.GetOrdinal("DetailID")),
                                BatchID = reader.GetInt32(reader.GetOrdinal("BatchID")),
                                AccountCode = reader.IsDBNull(reader.GetOrdinal("AccountCode")) ? string.Empty : reader.GetString(reader.GetOrdinal("AccountCode")),
                                DetectedName = reader.IsDBNull(reader.GetOrdinal("DetectedName")) ? string.Empty : reader.GetString(reader.GetOrdinal("DetectedName")),
                                ExcelBalance = reader.GetDecimal(reader.GetOrdinal("ExcelBalance")),
                                SystemBalanceAtTime = reader.IsDBNull(reader.GetOrdinal("SystemBalanceAtTime")) ? 0 : reader.GetDecimal(reader.GetOrdinal("SystemBalanceAtTime")),
                                ErrorMessage = reader.IsDBNull(reader.GetOrdinal("ErrorMessage")) ? null : reader.GetString(reader.GetOrdinal("ErrorMessage"))
                            });
                        }
                    }
                }
            }

            return details;
        }

        public BatchStatistics GetBatchStatistics()
        {
            var statistics = new BatchStatistics();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("sp_GetBatchStatistics", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            statistics.TotalBatches = reader.GetInt32(reader.GetOrdinal("TotalBatches"));
                            statistics.ProcessedCount = reader.IsDBNull(reader.GetOrdinal("ProcessedCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("ProcessedCount"));
                            statistics.PendingCount = reader.IsDBNull(reader.GetOrdinal("PendingCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("PendingCount"));
                            statistics.RejectedCount = reader.IsDBNull(reader.GetOrdinal("RejectedCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("RejectedCount"));
                            statistics.TotalRecords = reader.IsDBNull(reader.GetOrdinal("TotalRecords")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalRecords"));
                            statistics.LastUploadDate = reader.IsDBNull(reader.GetOrdinal("LastUploadDate")) ? null : reader.GetDateTime(reader.GetOrdinal("LastUploadDate"));
                        }
                    }
                }
            }

            return statistics;
        }

        public bool UpdateBatchStatus(int batchId, string newStatus)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateBatchStatus", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@BatchID", batchId);
                    conn.Open();

                    int rowsAffected = (int)cmd.ExecuteScalar();
                    return rowsAffected > 0;
                }
            }
        }

        public bool DeleteBatch(int batchId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_DeleteBatch", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BatchID", batchId);
                    
                    try
                    {
                        int rowsAffected = (int)cmd.ExecuteScalar();
                        return rowsAffected > 0;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public (int SuccessCount, int ErrorCount, string Message) ParseAndImportReport(string fileContent, string fileName, DateTime fileTimestamp, int userId)
        {
            int successCount = 0;
            int errorCount = 0;
            int batchId = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. ADIM: Batch Oluştur ve Tarih Kontrolü Yap
                    using (SqlCommand cmd = new SqlCommand("sp_ValidateAndCreateBatch", conn, transaction))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@FileName", fileName);
                        cmd.Parameters.AddWithValue("@FileTimestamp", fileTimestamp);
                        cmd.Parameters.AddWithValue("@UploadedBy", userId);
                        cmd.Parameters.AddWithValue("@TotalRecords", 0); // Şimdilik 0, döngü bitince güncellenebilir

                        // Output parametresi
                        SqlParameter outParam = new SqlParameter("@BatchID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outParam);

                        cmd.ExecuteNonQuery();

                        // Hata alınmadıysa, BatchID'nin anlamlı bir değer döndüğünden emin ol
                        if (outParam.Value == DBNull.Value || outParam.Value == null)
                        {
                            throw new Exception("Batch oluşturulamadı, BatchID döndürülmedi.");
                        }

                        batchId = (int)outParam.Value;

                        if (batchId <= 0)
                        {
                            throw new Exception("Geçersiz BatchID alındı (0 veya negatif). İşlem iptal edildi.");
                        }
                    }

                    // 2. ADIM: Excel İçeriğini Parse Et ve ImportDetails Tablosuna Ekle
                    // Not: Regex parse mantığın aynı kalacak, sadece INSERT hedefi değişiyor.
                    var chunks = Regex.Split(fileContent, @"(?=Cari\s*Kodu)", RegexOptions.IgnoreCase);

                    foreach (var chunk in chunks)
                    {
                        if (string.IsNullOrWhiteSpace(chunk) || !chunk.Trim().StartsWith("Cari", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var customer = ExtractCustomerFromChunk(chunk); // Mevcut metodun
                        if (customer != null && !string.IsNullOrWhiteSpace(customer.AccountCode))
                        {
                            // Customers tablosuna değil, ImportDetails tablosuna ekle
                            string insertDetail = @"INSERT INTO ImportDetails (BatchID, AccountCode, DetectedName, ExcelBalance) 
                                           VALUES (@BatchID, @AccountCode, @DetectedName, @ExcelBalance)";

                            using (SqlCommand detailCmd = new SqlCommand(insertDetail, conn, transaction))
                            {
                                detailCmd.Parameters.AddWithValue("@BatchID", batchId);
                                detailCmd.Parameters.AddWithValue("@AccountCode", customer.AccountCode);
                                detailCmd.Parameters.AddWithValue("@DetectedName", customer.CompanyName ?? string.Empty);
                                detailCmd.Parameters.AddWithValue("@ExcelBalance", customer.CurrentBalance);
                                detailCmd.ExecuteNonQuery();
                            }
                            successCount++;
                        }
                        else
                        {
                            errorCount++;
                        }
                    }

                    // 3. ADIM: Batch'in TotalRecords'unu güncelle
                    int totalRecords = successCount + errorCount;
                    if (totalRecords > 0)
                    {
                        using (SqlCommand updateCmd = new SqlCommand("sp_UpdateBatchTotalRecords", conn, transaction))
                        {
                            updateCmd.CommandType = CommandType.StoredProcedure;
                            updateCmd.Parameters.AddWithValue("@TotalRecords", totalRecords);
                            updateCmd.Parameters.AddWithValue("@BatchID", batchId);
                            updateCmd.ExecuteNonQuery();
                        }
                    }

                    // 4. ADIM: Eşitleme Prosedürünü Tetikle (Reconciliation)
                    using (SqlCommand procCmd = new SqlCommand("sp_ProcessReconciliation", conn, transaction))
                    {
                        procCmd.CommandType = CommandType.StoredProcedure;
                        procCmd.Parameters.AddWithValue("@BatchID", batchId);
                        procCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return (successCount, errorCount, "İşlem başarıyla tamamlandı.");
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    // Stored Procedure'den gelen "Eski Tarihli Dosya" hatasını yakala
                    // RAISERROR ile fırlatılan özel hatalar genellikle 50000-50999 arasındadır
                    // Ayrıca mesaj içeriğine göre de kontrol ediyoruz (daha güvenli)
                    if ((ex.Number >= 50000 && ex.Number <= 50999) || 
                        ex.Message.Contains("daha güncel bir veri zaten yüklü"))
                    {
                        throw new Exception(ex.Message);
                    }
                    throw;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private Customer? ExtractCustomerFromChunk(string chunk)
        {
            var customer = new Customer();

            var codeMatch = Regex.Match(chunk, @"Cari\s*Kodu\s*[:\s]*([A-Za-z0-9\-\.]+)", RegexOptions.IgnoreCase);
            if (codeMatch.Success)
            {
                customer.AccountCode = codeMatch.Groups[1].Value.Trim();
            }
            else
            {
                return null;
            }

            var nameMatch = Regex.Match(chunk, @"Cari\s*Ad[ıi]\s*[:\s]*(.+?)(?=\s*(?:Alacak|Borç|Telefon|Adres|Risk|$))", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (nameMatch.Success)
            {
                customer.CompanyName = CleanText(nameMatch.Groups[1].Value);
            }
            else
            {
                var altNameMatch = Regex.Match(chunk, @"Cari\s*Kodu\s*[:\s]*[A-Za-z0-9\-\.]+\s+(.+?)(?=\s*\n)", RegexOptions.IgnoreCase);
                if (altNameMatch.Success)
                {
                    customer.CompanyName = CleanText(altNameMatch.Groups[1].Value);
                }
            }

            if (string.IsNullOrWhiteSpace(customer.CompanyName))
            {
                customer.CompanyName = "Bilinmeyen Firma";
            }

            var phoneMatch = Regex.Match(chunk, @"Telefon\s*[:\s]*([\d\s\-\(\)]+?)(?=\s*(?:Borç|Adres|Fax|$))", RegexOptions.IgnoreCase);
            if (phoneMatch.Success)
            {
                var phone = Regex.Replace(phoneMatch.Groups[1].Value, @"[^\d]", "");
                if (phone.Length >= 7 && phone.Length <= 15)
                {
                    customer.PhoneNumber = FormatPhoneNumber(phone);
                }
            }

            var addressMatch = Regex.Match(chunk, @"Adres\s*[:\s]*(.+?)(?=\s*(?:#|Belge|Tarih|Borç|Telefon|\d{2}\.\d{2}\.\d{4}|$))", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (addressMatch.Success)
            {
                customer.Address = CleanText(addressMatch.Groups[1].Value);
            }

            var taxMatch = Regex.Match(chunk, @"Vergi\s*(?:No|Numarası|Kimlik)\s*[:\s]*(\d{10,11})", RegexOptions.IgnoreCase);
            if (taxMatch.Success)
            {
                customer.TaxID = taxMatch.Groups[1].Value.Trim();
            }

            var taxOfficeMatch = Regex.Match(chunk, @"Vergi\s*Dairesi\s*[:\s]*([^\n\r]+)", RegexOptions.IgnoreCase);
            if (taxOfficeMatch.Success)
            {
                customer.TaxOffice = CleanText(taxOfficeMatch.Groups[1].Value);
            }

            customer.CurrentBalance = CalculateBalance(chunk);
            customer.RiskLimit = 0;

            return customer;
        }

        private decimal CalculateBalance(string chunk)
        {
            decimal borcBakiye = 0;
            decimal alacakBakiye = 0;

            var borcMatch = Regex.Match(chunk, @"Borç\s*Bakiye\s*[:\s]*([\d\.,\s]+)", RegexOptions.IgnoreCase);
            if (borcMatch.Success)
            {
                borcBakiye = ParseTurkishDecimal(borcMatch.Groups[1].Value);
            }

            var alacakMatch = Regex.Match(chunk, @"Alacak\s*Bakiye\s*[:\s]*([\d\.,\s]+)", RegexOptions.IgnoreCase);
            if (alacakMatch.Success)
            {
                alacakBakiye = ParseTurkishDecimal(alacakMatch.Groups[1].Value);
            }

            return borcBakiye - alacakBakiye;
        }

        private decimal ParseTurkishDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0;
            value = value.Trim().Replace(" ", "");
            value = value.Replace(".", "").Replace(",", ".");
            if (decimal.TryParse(value, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            return 0;
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            text = Regex.Replace(text, @"[\r\n\t]+", " ");
            text = Regex.Replace(text, @"\s{2,}", " ");
            text = text.Replace("'", "''");
            return text.Trim();
        }

        private string FormatPhoneNumber(string digits)
        {
            if (digits.Length == 10)
                return $"0{digits.Substring(0,3)} {digits.Substring(3,3)} {digits.Substring(6,4)}";
            if (digits.Length == 11)
                return $"{digits.Substring(0,4)} {digits.Substring(4,3)} {digits.Substring(7,4)}";
            return digits;
        }

        private void SaveCustomerDirect(SqlConnection conn, Customer customer)
        {
            string checkQuery = "SELECT COUNT(*) FROM Customers WHERE AccountCode = @AccountCode";
            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
            {
                checkCmd.Parameters.AddWithValue("@AccountCode", customer.AccountCode);
                int exists = (int)checkCmd.ExecuteScalar();

                if (exists > 0)
                {
                    string updateQuery = @"UPDATE Customers SET 
                                           CompanyName = @CompanyName,
                                           TaxID = @TaxID,
                                           TaxOffice = @TaxOffice,
                                           Address = @Address,
                                           PhoneNumber = @PhoneNumber,
                                           CurrentBalance = @CurrentBalance
                                           WHERE AccountCode = @AccountCode";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        AddCustomerParameters(cmd, customer);
                        cmd.Parameters.AddWithValue("@CurrentBalance", customer.CurrentBalance);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string insertQuery = @"INSERT INTO Customers 
                                          (AccountCode, CompanyName, TaxID, TaxOffice, Address, PhoneNumber, RiskLimit, CurrentBalance) 
                                          VALUES 
                                          (@AccountCode, @CompanyName, @TaxID, @TaxOffice, @Address, @PhoneNumber, @RiskLimit, @CurrentBalance)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        AddCustomerParameters(cmd, customer);
                        cmd.Parameters.AddWithValue("@RiskLimit", customer.RiskLimit);
                        cmd.Parameters.AddWithValue("@CurrentBalance", customer.CurrentBalance);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void AddCustomerParameters(SqlCommand cmd, Customer customer)
        {
            cmd.Parameters.AddWithValue("@AccountCode", customer.AccountCode);
            cmd.Parameters.AddWithValue("@CompanyName", customer.CompanyName);
            cmd.Parameters.AddWithValue("@TaxID", (object?)customer.TaxID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TaxOffice", (object?)customer.TaxOffice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", (object?)customer.Address ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PhoneNumber", (object?)customer.PhoneNumber ?? DBNull.Value);
        }

        /// <summary>
        /// Müşteri işlem geçmişini getirir (sp_GetCustomerStatement stored procedure ile)
        /// </summary>
        public List<Transaction> GetCustomerStatement(int customerId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = new List<Transaction>();

            // Varsayılan olarak son 30 günlük işlemleri getir
            if (startDate == null)
            {
                startDate = DateTime.Now.AddDays(-30);
            }
            if (endDate == null)
            {
                endDate = DateTime.Now;
            }

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetCustomerStatement", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CustomerID", customerId);
                    cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
                    cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);

                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transactions.Add(new Transaction
                            {
                                TransactionID = reader.GetInt32(reader.GetOrdinal("TransactionID")),
                                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                                MethodID = reader.GetInt32(reader.GetOrdinal("MethodID")),
                                MethodName = reader.GetString(reader.GetOrdinal("MethodName")),
                                AccountID = reader.IsDBNull(reader.GetOrdinal("AccountID")) ? null : reader.GetInt32(reader.GetOrdinal("AccountID")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate")),
                                CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy"))
                            });
                        }
                    }
                }
            }

            return transactions;
        }
    }
}
