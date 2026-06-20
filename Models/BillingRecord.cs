using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ksp.care.Models
{
    public class BillingRecord
    {
        [Key]
        public int    Id            { get; set; }

        public int?   PatientId     { get; set; }

        [Required]
        public string PatientMobile { get; set; } = string.Empty;

        [Required]
        public string PatientName   { get; set; } = string.Empty;

        public int?   AppointmentId { get; set; }

        [Required]
        public string PackageName   { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 9999999.99)]
        public decimal TotalAmount  { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 9999999.99)]
        public decimal PaidAmount   { get; set; } = 0;

        [NotMapped]
        public decimal PendingAmount => TotalAmount - PaidAmount;

        // Pending | Paid
        public string Status        { get; set; } = "Pending";
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
    }
}
