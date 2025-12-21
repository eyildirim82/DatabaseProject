using Microsoft.AspNetCore.Mvc;
using DatabaseProject.DAL;
using DatabaseProject.Models;

namespace DatabaseProject.Controllers
{
    public class AuditController : Controller
    {
        private readonly AuditDAL _auditDAL;

        public AuditController(IConfiguration configuration)
        {
            _auditDAL = new AuditDAL(configuration);
        }

        // Admin yetki kontrolü
        private bool IsAdmin()
        {
            // #region agent log
            try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "AuditController.cs:19", message = "IsAdmin entry", data = new { sessionExists = HttpContext.Session != null }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            var roleId = HttpContext.Session?.GetInt32("RoleID");
            // #region agent log
            try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "AuditController.cs:22", message = "IsAdmin roleId value", data = new { roleId = roleId, isAdmin = roleId == 1 }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            // RoleID 1 = Admin (genellikle)
            return roleId == 1;
        }

        // GET: Audit
        public IActionResult Index(
            string? tableNameFilter,
            string? operationTypeFilter,
            int? changedByFilter,
            DateTime? startDate,
            DateTime? endDate,
            int page = 1,
            int pageSize = 20,
            string sortBy = "LogDate",
            string sortDirection = "DESC")
        {
            // #region agent log
            try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "AuditController.cs:35", message = "Index entry", data = new { tableNameFilter, operationTypeFilter, changedByFilter, startDate, endDate, page, pageSize, sortBy, sortDirection }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
            // #endregion
            // Admin kontrolü
            if (!IsAdmin())
            {
                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "C", location = "AuditController.cs:40", message = "Not admin redirect", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz bulunmamaktadır. Sadece Admin kullanıcıları erişebilir.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuditController.cs:48", message = "Before GetSystemLogs", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                int totalRecords;
                var logs = _auditDAL.GetSystemLogs(
                    out totalRecords,
                    tableNameFilter,
                    operationTypeFilter,
                    changedByFilter,
                    startDate,
                    endDate,
                    page,
                    pageSize,
                    sortBy,
                    sortDirection);
                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuditController.cs:60", message = "After GetSystemLogs", data = new { logsCount = logs?.Count ?? 0, totalRecords }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion

                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "AuditController.cs:64", message = "Before GetDistinctTableNames", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                var tableNames = _auditDAL.GetDistinctTableNames();
                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "AuditController.cs:68", message = "After GetDistinctTableNames", data = new { tableNamesCount = tableNames?.Count ?? 0, tableNamesIsNull = tableNames == null }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "AuditController.cs:70", message = "Before GetDistinctOperationTypes", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                var operationTypes = _auditDAL.GetDistinctOperationTypes();
                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "AuditController.cs:73", message = "After GetDistinctOperationTypes", data = new { operationTypesCount = operationTypes?.Count ?? 0, operationTypesIsNull = operationTypes == null }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion

                var viewModel = new AuditLogViewModel
                {
                    Logs = logs,
                    TableNameFilter = tableNameFilter,
                    OperationTypeFilter = operationTypeFilter,
                    ChangedByFilter = changedByFilter,
                    StartDate = startDate,
                    EndDate = endDate,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    SortBy = sortBy,
                    SortDirection = sortDirection,
                    AvailableTableNames = tableNames,
                    AvailableOperationTypes = operationTypes
                };
                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "AuditController.cs:90", message = "ViewModel created", data = new { viewModelAvailableTableNamesIsNull = viewModel.AvailableTableNames == null, viewModelAvailableOperationTypesIsNull = viewModel.AvailableOperationTypes == null }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // #region agent log
                try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "AuditController.cs:95", message = "Exception caught", data = new { exceptionType = ex.GetType().Name, exceptionMessage = ex.Message, exceptionStackTrace = ex.StackTrace }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                // #endregion
                TempData["ErrorMessage"] = $"Audit log yüklenirken hata oluştu: {ex.Message}";
                try
                {
                    // #region agent log
                    try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "AuditController.cs:100", message = "Before GetDistinctTableNames in catch", data = new { }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
                    var errorTableNames = _auditDAL.GetDistinctTableNames();
                    // #region agent log
                    try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "AuditController.cs:103", message = "After GetDistinctTableNames in catch", data = new { errorTableNamesCount = errorTableNames?.Count ?? 0 }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
                    var errorOperationTypes = _auditDAL.GetDistinctOperationTypes();
                    return View(new AuditLogViewModel
                    {
                        AvailableTableNames = errorTableNames,
                        AvailableOperationTypes = errorOperationTypes
                    });
                }
                catch (Exception innerEx)
                {
                    // #region agent log
                    try { System.IO.File.AppendAllText(@"c:\Users\erkan\source\repos\DatabaseProject\.cursor\debug.log", System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "AuditController.cs:111", message = "Inner exception in catch", data = new { innerExceptionType = innerEx.GetType().Name, innerExceptionMessage = innerEx.Message }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n"); } catch { }
                    // #endregion
                    return View(new AuditLogViewModel());
                }
            }
        }
    }
}
