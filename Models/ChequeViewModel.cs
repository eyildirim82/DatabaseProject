namespace DatabaseProject.Models
{
    public class ChequeViewModel
    {
        public List<Cheque> Cheques { get; set; } = new List<Cheque>();
        public string? StatusFilter { get; set; }
        public List<string> AvailableStatuses { get; set; } = new List<string> 
        { 
            "Portfolio", 
            "Collected", 
            "Bounced", 
            "Returned" 
        };
    }
}
