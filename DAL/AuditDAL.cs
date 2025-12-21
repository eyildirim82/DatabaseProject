using Microsoft.Data.SqlClient;
using System.Data;
using DatabaseProject.Models;

namespace DatabaseProject.DAL
{
    public class AuditDAL
    {
        private readonly string _connectionString;

        public AuditDAL(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        /// <summary>
        /// SystemLogs tablosundan veri çeker (filtreleme, sayfalama, sıralama ile)
        /// </summary>
        public List<SystemLog> GetSystemLogs(
            out int totalRecords,
            string? tableNameFilter = null,
            string? operationTypeFilter = null,
            int? changedByFilter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1,
            int pageSize = 20,
            string sortBy = "LogDate",
            string sortDirection = "DESC")
        {
            var logs = new List<SystemLog>();
            totalRecords = 0;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();

                // Önce toplam kayıt sayısını al
                string countQuery = @"SELECT COUNT(*) 
                                     FROM SystemLogs 
                                     WHERE (@TableName IS NULL OR TableName = @TableName)
                                       AND (@OperationType IS NULL OR OperationType = @OperationType)
                                       AND (@ChangedBy IS NULL OR ChangedBy = @ChangedBy)
                                       AND (@StartDate IS NULL OR LogDate >= @StartDate)
                                       AND (@EndDate IS NULL OR LogDate <= @EndDate)";

                using (SqlCommand countCmd = new SqlCommand(countQuery, conn))
                {
                    countCmd.Parameters.AddWithValue("@TableName", (object?)tableNameFilter ?? DBNull.Value);
                    countCmd.Parameters.AddWithValue("@OperationType", (object?)operationTypeFilter ?? DBNull.Value);
                    countCmd.Parameters.AddWithValue("@ChangedBy", (object?)changedByFilter ?? DBNull.Value);
                    countCmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
                    countCmd.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);

                    totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());
                }

                // Sonra sayfalanmış verileri al
                int offset = (page - 1) * pageSize;
                string sortColumn = sortBy switch
                {
                    "TableName" => "TableName",
                    "OperationType" => "OperationType",
                    "LogDate" => "LogDate",
                    _ => "LogDate"
                };

                string query = $@"SELECT LogID, TableName, RecordID, OperationType, OldValue, NewValue, ChangedBy, LogDate
                                 FROM SystemLogs 
                                 WHERE (@TableName IS NULL OR TableName = @TableName)
                                   AND (@OperationType IS NULL OR OperationType = @OperationType)
                                   AND (@ChangedBy IS NULL OR ChangedBy = @ChangedBy)
                                   AND (@StartDate IS NULL OR LogDate >= @StartDate)
                                   AND (@EndDate IS NULL OR LogDate <= @EndDate)
                                 ORDER BY {sortColumn} {(sortDirection == "ASC" ? "ASC" : "DESC")}
                                 OFFSET @Offset ROWS
                                 FETCH NEXT @PageSize ROWS ONLY";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TableName", (object?)tableNameFilter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OperationType", (object?)operationTypeFilter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ChangedBy", (object?)changedByFilter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new SystemLog
                            {
                                LogID = reader.GetInt32(reader.GetOrdinal("LogID")),
                                TableName = reader.GetString(reader.GetOrdinal("TableName")),
                                RecordID = reader.GetInt32(reader.GetOrdinal("RecordID")),
                                OperationType = reader.GetString(reader.GetOrdinal("OperationType")),
                                OldValue = reader.IsDBNull(reader.GetOrdinal("OldValue")) ? null : reader.GetString(reader.GetOrdinal("OldValue")),
                                NewValue = reader.IsDBNull(reader.GetOrdinal("NewValue")) ? null : reader.GetString(reader.GetOrdinal("NewValue")),
                                ChangedBy = reader.IsDBNull(reader.GetOrdinal("ChangedBy")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ChangedBy")),
                                LogDate = reader.GetDateTime(reader.GetOrdinal("LogDate"))
                            });
                        }
                    }
                }
            }

            return logs;
        }

        /// <summary>
        /// Tüm tablo isimlerini getirir (filtreleme için)
        /// </summary>
        public List<string> GetDistinctTableNames()
        {
            // #region agent log
            try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuditDAL.cs:116", message = "GetDistinctTableNames entry", data = new { connectionStringExists = !string.IsNullOrEmpty(_connectionString) }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            var tableNames = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT DISTINCT TableName FROM SystemLogs ORDER BY TableName";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // #region agent log
                        try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuditDAL.cs:125", message = "Before connection open", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                        // #endregion
                        conn.Open();
                        // #region agent log
                        try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuditDAL.cs:128", message = "After connection open", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                        // #endregion

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // #region agent log
                            try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuditDAL.cs:132", message = "Before reader loop", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                            // #endregion
                            while (reader.Read())
                            {
                                tableNames.Add(reader.GetString(reader.GetOrdinal("TableName")));
                            }
                            // #region agent log
                            try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuditDAL.cs:137", message = "After reader loop", data = new { tableNamesCount = tableNames.Count }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                            // #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuditDAL.cs:144", message = "Exception in GetDistinctTableNames", data = new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                throw;
            }

            return tableNames;
        }

        /// <summary>
        /// Tüm operation type'ları getirir (filtreleme için)
        /// </summary>
        public List<string> GetDistinctOperationTypes()
        {
            // #region agent log
            try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuditDAL.cs:170", message = "GetDistinctOperationTypes entry", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            var operationTypes = new List<string>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "SELECT DISTINCT OperationType FROM SystemLogs ORDER BY OperationType";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                operationTypes.Add(reader.GetString(reader.GetOrdinal("OperationType")));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuditDAL.cs:192", message = "Exception in GetDistinctOperationTypes", data = new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                throw;
            }

            return operationTypes;
        }
    }
}
