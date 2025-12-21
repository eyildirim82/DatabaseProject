namespace DatabaseProject.Models
{
    public class BatchStatistics
    {
        public int TotalBatches { get; set; }
        public int ProcessedCount { get; set; }
        public int PendingCount { get; set; }
        public int RejectedCount { get; set; }
        public int TotalRecords { get; set; }
        public DateTime? LastUploadDate { get; set; }
    }
}
