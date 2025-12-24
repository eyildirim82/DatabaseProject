using System.Data;
using Microsoft.Data.SqlClient;
using DatabaseProject.Models;

namespace DatabaseProject.DAL
{
    public class TransactionDAL
    {
        private readonly string _connectionString;

        public TransactionDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        // Ödeme Metotlarını Doldurmak İçin (Dropdown)
        public DataTable GetPaymentMethods()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetPaymentMethods", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    conn.Open();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        // Şirket Hesaplarını Doldurmak İçin (Dropdown)
        public DataTable GetCompanyAccounts()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetCompanyAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    conn.Open();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        // İŞLEM EKLEME (SP Çağrısı)
        public void AddTransaction(int customerId, int methodId, int accountId, decimal amount, string description, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // Step 3'te yazdığımız SP ismi: sp_AddTransaction
                using (SqlCommand cmd = new SqlCommand("sp_AddTransaction", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CustomerID", customerId);
                    cmd.Parameters.AddWithValue("@MethodID", methodId);
                    cmd.Parameters.AddWithValue("@AccountID", accountId);
                    cmd.Parameters.AddWithValue("@Amount", amount);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);

                    conn.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        // SQL Server'dan gelen özel hataları yakala (Risk Limiti vb.)
                        // Hata mesajını kullanıcı dostu hale getir
                        string errorMessage = ex.Message;
                        if (errorMessage.Contains("Risk Limit") || errorMessage.Contains("Limit Exceeded"))
                        {
                            errorMessage = "İşlem reddedildi: Müşteri risk limiti aşıldı. Lütfen müşteri risk limitini kontrol edin.";
                        }
                        else if (errorMessage.Contains("CHECK constraint"))
                        {
                            errorMessage = "İşlem reddedildi: Geçersiz veri girişi. Lütfen girdiğiniz değerleri kontrol edin.";
                        }
                        else
                        {
                            errorMessage = $"İşlem sırasında hata oluştu: {ex.Message}";
                        }
                        throw new Exception(errorMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Tüm transaction'ları getirir (filtreleme ile)
        /// </summary>
        public List<Transaction> GetAllTransactions(int? customerId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var transactions = new List<Transaction>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllTransactions", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerID", (object?)customerId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);
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
                                AccountID = reader.IsDBNull(reader.GetOrdinal("AccountID")) ? null : reader.GetInt32(reader.GetOrdinal("AccountID")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                TransactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate")),
                                CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                                CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                                MethodName = reader.GetString(reader.GetOrdinal("MethodName")),
                                BankName = reader.IsDBNull(reader.GetOrdinal("BankName")) ? null : reader.GetString(reader.GetOrdinal("BankName")),
                                FullName = reader.GetString(reader.GetOrdinal("FullName"))
                            });
                        }
                    }
                }
            }

            return transactions;
        }

        /// <summary>
        /// Transaction'ı iptal eder (sp_ReverseTransaction stored procedure ile)
        /// Tersine çevrilmiş yeni transaction oluşturur
        /// </summary>
        public void ReverseTransaction(int transactionId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_ReverseTransaction", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@TransactionID", transactionId);
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    conn.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Transaction iptal edilirken hata oluştu: {ex.Message}");
                    }
                }
            }
        }
    }
}
