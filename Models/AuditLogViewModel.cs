namespace DatabaseProject.Models
{
    public class AuditLogViewModel
    {
        public List<SystemLog> Logs { get; set; } = new List<SystemLog>();
        
        // Filtreleme parametreleri
        public string? TableNameFilter { get; set; }
        public string? OperationTypeFilter { get; set; }
        public int? ChangedByFilter { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Sayfalama parametreleri
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        
        // Sıralama parametreleri
        public string SortBy { get; set; } = "LogDate";
        public string SortDirection { get; set; } = "DESC";
        
        // Dropdown listeleri için
        public List<string> AvailableTableNames { get; set; } = new List<string>();
        public List<string> AvailableOperationTypes { get; set; } = new List<string>();
    }
}
