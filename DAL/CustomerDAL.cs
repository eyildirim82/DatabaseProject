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
                string query = @"SELECT CustomerID, AccountCode, CompanyName, TaxID, TaxOffice, 
                                 Address, PhoneNumber, RiskLimit, CurrentBalance, AvailableRisk 
                                 FROM Customers ORDER BY CompanyName";
                
                SqlCommand cmd = new SqlCommand(query, conn);
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

            return customers;
        }

        public List<ImportBatch> GetImportHistory()
        {
            return GetImportHistory(null, null, null, null, 1, 10, "UploadDate", "DESC", out _);
        }

        public List<ImportBatch> GetImportHistory(string? statusFilter, string? fileNameFilter, 
            DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, 
            string sortBy, string sortDirection, out int totalRecords)
        {
            var batches = new List<ImportBatch>();
            totalRecords = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Önce toplam kayıt sayısını al
                string countQuery = @"SELECT COUNT(*) FROM ImportBatches 
                                     WHERE (@Status IS NULL OR Status = @Status)
                                       AND (@FileName IS NULL OR FileName LIKE '%' + @FileName + '%')
                                       AND (@StartDate IS NULL OR UploadDate >= @StartDate)
                                       AND (@EndDate IS NULL OR UploadDate <= @EndDate)";

                using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
                {
                    countCmd.Parameters.AddWithValue("@Status", (object?)statusFilter ?? DBNull.Value);
                    countCmd.Parameters.AddWithValue("@FileName", (object?)fileNameFilter ?? DBNull.Value);
                    countCmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
                    countCmd.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);
                    totalRecords = (int)countCmd.ExecuteScalar();
                }

                // Sonra sayfalanmış ve sıralanmış verileri al
                int offset = (pageNumber - 1) * pageSize;
                string sortColumn = sortBy switch
                {
                    "FileName" => "FileName",
                    "FileTimestamp" => "FileTimestamp",
                    "TotalRecords" => "TotalRecords",
                    "Status" => "Status",
                    _ => "UploadDate"
                };

                string query = $@"SELECT BatchID, FileName, FileTimestamp, UploadDate, TotalRecords, Status 
                                 FROM ImportBatches 
                                 WHERE (@Status IS NULL OR Status = @Status)
                                   AND (@FileName IS NULL OR FileName LIKE '%' + @FileName + '%')
                                   AND (@StartDate IS NULL OR UploadDate >= @StartDate)
                                   AND (@EndDate IS NULL OR UploadDate <= @EndDate)
                                 ORDER BY {sortColumn} {(sortDirection == "ASC" ? "ASC" : "DESC")}
                                 OFFSET @Offset ROWS
                                 FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", (object?)statusFilter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileName", (object?)fileNameFilter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

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
                }
            }

            return batches;
        }

        public List<ImportDetail> GetBatchDetails(int batchId)
        {
            var details = new List<ImportDetail>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT DetailID, BatchID, AccountCode, DetectedName, ExcelBalance, SystemBalanceAtTime
                                FROM ImportDetails
                                WHERE BatchID = @BatchID
                                ORDER BY AccountCode";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
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
                                SystemBalanceAtTime = reader.IsDBNull(reader.GetOrdinal("SystemBalanceAtTime")) ? 0 : reader.GetDecimal(reader.GetOrdinal("SystemBalanceAtTime"))
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

                // Toplam batch sayısı ve status'lere göre sayılar
                string query = @"SELECT 
                                    COUNT(*) as TotalBatches,
                                    SUM(CASE WHEN Status = 'Processed' THEN 1 ELSE 0 END) as ProcessedCount,
                                    SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) as PendingCount,
                                    SUM(CASE WHEN Status = 'Rejected' THEN 1 ELSE 0 END) as RejectedCount,
                                    SUM(TotalRecords) as TotalRecords,
                                    MAX(UploadDate) as LastUploadDate
                                 FROM ImportBatches";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
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
                string query = "UPDATE ImportBatches SET Status = @Status WHERE BatchID = @BatchID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@BatchID", batchId);
                    conn.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool DeleteBatch(int batchId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // Önce ImportDetails kayıtlarını sil
                    string deleteDetailsQuery = "DELETE FROM ImportDetails WHERE BatchID = @BatchID";
                    using (SqlCommand detailCmd = new SqlCommand(deleteDetailsQuery, conn, transaction))
                    {
                        detailCmd.Parameters.AddWithValue("@BatchID", batchId);
                        detailCmd.ExecuteNonQuery();
                    }

                    // Sonra ImportBatch kaydını sil
                    string deleteBatchQuery = "DELETE FROM ImportBatches WHERE BatchID = @BatchID";
                    using (SqlCommand batchCmd = new SqlCommand(deleteBatchQuery, conn, transaction))
                    {
                        batchCmd.Parameters.AddWithValue("@BatchID", batchId);
                        int rowsAffected = batchCmd.ExecuteNonQuery();
                        
                        transaction.Commit();
                        return rowsAffected > 0;
                    }
                }
                catch
                {
                    transaction.Rollback();
                    return false;
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

                    // 3. ADIM: Eşitleme Prosedürünü Tetikle (Reconciliation)
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
    }
}
