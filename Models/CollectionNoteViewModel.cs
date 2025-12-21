namespace DatabaseProject.Models
{
    public class CollectionNoteViewModel
    {
        public List<CollectionNote> CollectionNotes { get; set; } = new List<CollectionNote>();
        public int? CustomerID { get; set; }
        public string? CustomerName { get; set; }
        public List<Customer>? AvailableCustomers { get; set; }
    }
}
