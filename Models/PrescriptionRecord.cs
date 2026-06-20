using System;
using System.ComponentModel.DataAnnotations;

namespace ksp.care.Models
{
    public class PrescriptionRecord
    {
        [Key]
        public int Id { get; set; }

        public int? AppointmentId { get; set; }

        [Required]
        public string PatientMobile { get; set; } = string.Empty;

        [Required]
        public string PatientName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string DoctorName { get; set; } = string.Empty;

        [StringLength(200)]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        public string Medicines { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
