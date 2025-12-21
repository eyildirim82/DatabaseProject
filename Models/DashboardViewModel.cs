namespace DatabaseProject.Models
{
    public class DashboardViewModel
    {
        public DashboardStats Stats { get; set; } = new DashboardStats();
        public List<RiskStatusItem> RiskStatusDistribution { get; set; } = new List<RiskStatusItem>();
        public List<DailyCashFlowItem> DailyCashFlow { get; set; } = new List<DailyCashFlowItem>();
        public List<CustomerRiskStatus> CustomerRiskStatuses { get; set; } = new List<CustomerRiskStatus>();
    }

    public class DashboardStats
    {
        public int TotalCustomers { get; set; }
        public decimal TodayCollection { get; set; }
        public int PendingCheques { get; set; }
    }

    public class RiskStatusItem
    {
        public string RiskStatus { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class DailyCashFlowItem
    {
        public DateTime PaymentDate { get; set; }
        public string MethodName { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CustomerRiskStatus
    {
        public int CustomerID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public decimal RiskLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal AvailableLimit { get; set; }
        public string RiskStatus { get; set; } = string.Empty;
    }
}
