namespace DatabaseProject.Models
{
    public class CollectionNote
    {
        public int NoteID { get; set; }
        public int CustomerID { get; set; }
        public int UserID { get; set; }
        public string NoteText { get; set; } = string.Empty;
        public DateTime? PromiseDate { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Join ile gelen alanlar
        public string CompanyName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
