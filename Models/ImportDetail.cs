namespace DatabaseProject.Models
{
    public class ImportDetail
    {
        public int DetailID { get; set; }
        public int BatchID { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string DetectedName { get; set; } = string.Empty;
        public decimal ExcelBalance { get; set; }
        public decimal SystemBalanceAtTime { get; set; }
        public string? ErrorMessage { get; set; }
        
        // Computed property
        public decimal BalanceDifference => ExcelBalance - SystemBalanceAtTime;
    }
}
