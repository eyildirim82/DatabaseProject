namespace DatabaseProject.Models
{
    public class SystemLog
    {
        public int LogID { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int RecordID { get; set; }
        public string OperationType { get; set; } = string.Empty; // INSERT, UPDATE, DELETE
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public int? ChangedBy { get; set; }
        public DateTime LogDate { get; set; }
    }
}
