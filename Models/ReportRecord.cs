using System;
using System.ComponentModel.DataAnnotations;

namespace ksp.care.Models
{
    public class ReportRecord
    {
        [Key]
        public int    Id            { get; set; }
        public string PatientMobile { get; set; } = string.Empty;
        public string PatientName   { get; set; } = string.Empty;
        public string TestName      { get; set; } = string.Empty;
        public string FilePath      { get; set; } = string.Empty;
        public DateTime UploadedAt  { get; set; } = DateTime.UtcNow;
    }
}
