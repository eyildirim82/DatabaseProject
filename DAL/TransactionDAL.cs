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
                using (SqlCommand cmd = new SqlCommand("SELECT MethodID, MethodName FROM PaymentMethods", conn))
                {
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
                using (SqlCommand cmd = new SqlCommand("SELECT AccountID, BankName FROM CompanyAccounts", conn))
                {
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
                        // Hoca bu hatayı arayüzde görmek isteyecek.
                        throw new Exception($"Veritabanı Hatası: {ex.Message}");
                    }
                }
            }
        }
    }
}
