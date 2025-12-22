using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseProject.Models;

namespace DatabaseProject.DAL
{
    public class DashboardDAL
    {
        private readonly string _connectionString;

        public DashboardDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        /// <summary>
        /// Dashboard özet istatistiklerini getirir (sp_GetDashboardStats)
        /// </summary>
        public DashboardStats GetDashboardStats()
        {
            var stats = new DashboardStats();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetDashboardStats", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stats.TotalCustomers = reader.GetInt32(reader.GetOrdinal("TotalCustomers"));
                            stats.TodayCollection = reader.GetDecimal(reader.GetOrdinal("TodayCollection"));
                            stats.PendingCheques = reader.GetInt32(reader.GetOrdinal("PendingCheques"));
                            stats.TotalReceivables = reader.GetDecimal(reader.GetOrdinal("TotalReceivables"));
                            stats.RiskyCustomerCount = reader.GetInt32(reader.GetOrdinal("RiskyCustomerCount"));
                        }
                    }
                }
            }

            return stats;
        }

        /// <summary>
        /// Risk durumu dağılımını getirir (vw_CustomerRiskStatus view'ından)
        /// </summary>
        public List<RiskStatusItem> GetRiskStatusDistribution()
        {
            var riskStatusList = new List<RiskStatusItem>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT RiskStatus, COUNT(*) as Count 
                                FROM vw_CustomerRiskStatus 
                                GROUP BY RiskStatus";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            riskStatusList.Add(new RiskStatusItem
                            {
                                RiskStatus = reader.GetString(reader.GetOrdinal("RiskStatus")),
                                Count = reader.GetInt32(reader.GetOrdinal("Count"))
                            });
                        }
                    }
                }
            }

            return riskStatusList;
        }

        /// <summary>
        /// Son 30 günlük nakit akışını getirir (vw_DailyCashFlow view'ından)
        /// </summary>
        public List<DailyCashFlowItem> GetDailyCashFlow(int days = 30)
        {
            var cashFlowList = new List<DailyCashFlowItem>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT PaymentDate, MethodName, TransactionCount, TotalAmount 
                                FROM vw_DailyCashFlow 
                                WHERE PaymentDate >= DATEADD(day, -@Days, GETDATE())
                                ORDER BY PaymentDate ASC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Days", days);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cashFlowList.Add(new DailyCashFlowItem
                            {
                                PaymentDate = reader.GetDateTime(reader.GetOrdinal("PaymentDate")),
                                MethodName = reader.GetString(reader.GetOrdinal("MethodName")),
                                TransactionCount = reader.GetInt32(reader.GetOrdinal("TransactionCount")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
                            });
                        }
                    }
                }
            }

            return cashFlowList;
        }

        /// <summary>
        /// Tüm risk durumu detaylarını getirir (grafik için)
        /// </summary>
        public List<CustomerRiskStatus> GetCustomerRiskStatuses()
        {
            var riskStatuses = new List<CustomerRiskStatus>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT CustomerID, CompanyName, RiskLimit, CurrentBalance, 
                                AvailableLimit, RiskStatus 
                                FROM vw_CustomerRiskStatus 
                                ORDER BY RiskStatus, CurrentBalance DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            riskStatuses.Add(new CustomerRiskStatus
                            {
                                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                                CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                                RiskLimit = reader.GetDecimal(reader.GetOrdinal("RiskLimit")),
                                CurrentBalance = reader.GetDecimal(reader.GetOrdinal("CurrentBalance")),
                                AvailableLimit = reader.GetDecimal(reader.GetOrdinal("AvailableLimit")),
                                RiskStatus = reader.GetString(reader.GetOrdinal("RiskStatus"))
                            });
                        }
                    }
                }
            }

            return riskStatuses;
        }

        /// <summary>
        /// Vadesi geçen çek sayısını getirir
        /// </summary>
        public int GetOverdueChequesCount()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string query = @"SELECT COUNT(*) 
                                FROM Cheques 
                                WHERE Status = 'Portfolio' 
                                  AND DueDate < GETDATE()";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
    }
}
