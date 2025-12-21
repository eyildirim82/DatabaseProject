using System;
using System.Collections.Generic;

namespace DatabaseProject.Models
{
    public class ImportBatchViewModel
    {
        public List<ImportBatch> Batches { get; set; } = new();
        
        // Filtreleme parametreleri
        public string? StatusFilter { get; set; }
        public string? FileNameFilter { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        
        // Sayfalama parametreleri
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        
        // Sıralama parametreleri
        public string SortBy { get; set; } = "UploadDate";
        public string SortDirection { get; set; } = "DESC";
        
        // İstatistikler
        public BatchStatistics? Statistics { get; set; }
    }
}
