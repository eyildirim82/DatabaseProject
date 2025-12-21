namespace DatabaseProject.Models
{
    public class Cheque
    {
        public int ChequeID { get; set; }
        public int CustomerID { get; set; }
        public string? CompanyName { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string? ChequeNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = "Portfolio"; // Portfolio, Collected, Bounced, Returned
        public DateTime ReceivedDate { get; set; }
        public int? DaysToMaturity { get; set; } // vw_PortfolioCheques'den gelir
    }
}
