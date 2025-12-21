namespace DatabaseProject.Models
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int CustomerID { get; set; }
        public int MethodID { get; set; }
        public int? AccountID { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public int CreatedBy { get; set; }
        
        // Join ile gelen alanlar
        public string CompanyName { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public string? BankName { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}
