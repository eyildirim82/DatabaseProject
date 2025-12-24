using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseProject.Models;

namespace DatabaseProject.DAL
{
    public class ChequeDAL
    {
        private readonly string _connectionString;

        public ChequeDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        /// <summary>
        /// Portfolio durumundaki çekleri getirir (vw_PortfolioCheques view'ından)
        /// </summary>
        public List<Cheque> GetPortfolioCheques()
        {
            var cheques = new List<Cheque>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetPortfolioCheques", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cheques.Add(new Cheque
                            {
                                ChequeID = reader.GetInt32(reader.GetOrdinal("ChequeID")),
                                CompanyName = reader.IsDBNull(reader.GetOrdinal("CompanyName")) ? null : reader.GetString(reader.GetOrdinal("CompanyName")),
                                BankName = reader.GetString(reader.GetOrdinal("BankName")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                                DaysToMaturity = reader.IsDBNull(reader.GetOrdinal("DaysToMaturity")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("DaysToMaturity"))
                            });
                        }
                    }
                }
            }

            return cheques;
        }

        /// <summary>
        /// Tüm çekleri status'a göre filtreleyerek getirir
        /// </summary>
        public List<Cheque> GetAllCheques(string? statusFilter = null)
        {
            var cheques = new List<Cheque>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetAllCheques", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Status", (object?)statusFilter ?? DBNull.Value);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cheques.Add(new Cheque
                            {
                                ChequeID = reader.GetInt32(reader.GetOrdinal("ChequeID")),
                                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                                CompanyName = reader.IsDBNull(reader.GetOrdinal("CompanyName")) ? null : reader.GetString(reader.GetOrdinal("CompanyName")),
                                BankName = reader.GetString(reader.GetOrdinal("BankName")),
                                ChequeNumber = reader.IsDBNull(reader.GetOrdinal("ChequeNumber")) ? null : reader.GetString(reader.GetOrdinal("ChequeNumber")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                ReceivedDate = reader.GetDateTime(reader.GetOrdinal("ReceivedDate"))
                            });
                        }
                    }
                }
            }

            return cheques;
        }

        /// <summary>
        /// Çek ekleme (sp_AddChequeWithRiskCheck stored procedure ile)
        /// </summary>
        public void AddCheque(int customerId, string bankName, decimal amount, DateTime dueDate, string? chequeNumber = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_AddChequeWithRiskCheck", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CustomerID", customerId);
                    cmd.Parameters.AddWithValue("@BankName", bankName);
                    cmd.Parameters.AddWithValue("@Amount", amount);
                    cmd.Parameters.AddWithValue("@DueDate", dueDate);

                    conn.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Çek eklenirken hata oluştu: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Çek durumunu günceller (sp_UpdateChequeStatus stored procedure ile)
        /// </summary>
        public void UpdateChequeStatus(int chequeId, string newStatus)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateChequeStatus", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ChequeID", chequeId);
                    cmd.Parameters.AddWithValue("@NewStatus", newStatus);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Çek ID'ye göre çek bilgilerini getirir
        /// </summary>
        public Cheque? GetChequeById(int chequeId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetChequeById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ChequeID", chequeId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Cheque
                            {
                                ChequeID = reader.GetInt32(reader.GetOrdinal("ChequeID")),
                                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                                CompanyName = reader.IsDBNull(reader.GetOrdinal("CompanyName")) ? null : reader.GetString(reader.GetOrdinal("CompanyName")),
                                BankName = reader.GetString(reader.GetOrdinal("BankName")),
                                ChequeNumber = reader.IsDBNull(reader.GetOrdinal("ChequeNumber")) ? null : reader.GetString(reader.GetOrdinal("ChequeNumber")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                ReceivedDate = reader.GetDateTime(reader.GetOrdinal("ReceivedDate"))
                            };
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Çek silme (soft delete için status güncelleme veya fiziksel silme)
        /// </summary>
        public void DeleteCheque(int chequeId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteCheque", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ChequeID", chequeId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Çek tahsil etme (sp_CollectCheque stored procedure ile)
        /// Transaction oluşturur ve müşteri bakiyesini günceller
        /// </summary>
        public void CollectCheque(int chequeId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_CollectCheque", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ChequeID", chequeId);
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    conn.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Çek tahsil edilirken hata oluştu: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Çek karşılıksız olarak işaretleme (sp_MarkChequeBounced stored procedure ile)
        /// </summary>
        public void MarkBounced(int chequeId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_MarkChequeBounced", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ChequeID", chequeId);
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    conn.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Çek karşılıksız olarak işaretlenirken hata oluştu: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Çek iade etme (sp_UpdateChequeStatus stored procedure ile)
        /// Status'u "Returned" yapar, trigger otomatik olarak bakiyeyi düzeltecek
        /// </summary>
        public void ReturnCheque(int chequeId, int userId)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateChequeStatus", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ChequeID", chequeId);
                    cmd.Parameters.AddWithValue("@NewStatus", "Returned");

                    conn.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception($"Çek iade edilirken hata oluştu: {ex.Message}");
                    }
                }
            }
        }
    }
}
