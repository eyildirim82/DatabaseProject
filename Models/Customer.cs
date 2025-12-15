namespace DatabaseProject.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? TaxID { get; set; }
        public string? TaxOffice { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal RiskLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        
        // Computed column from database
        public decimal AvailableRisk => RiskLimit - CurrentBalance;
    }
}
