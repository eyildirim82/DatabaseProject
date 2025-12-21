namespace DatabaseProject.Models
{
    public class ImportBatch
    {
        public int BatchID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime FileTimestamp { get; set; }
        public DateTime UploadDate { get; set; }
        public int TotalRecords { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
