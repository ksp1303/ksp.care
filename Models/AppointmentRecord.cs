using System;
using System.ComponentModel.DataAnnotations;

namespace ksp.care.Models
{
    public class AppointmentRecord
    {
        [Key]
        public int    Id              { get; set; }

        // Stable link to the patient profile this booking belongs to.
        // The booking (Id) differs every time; PatientId stays the same per patient.
        public int?   PatientId       { get; set; }

        [Required]
        public string PatientMobile   { get; set; } = string.Empty;

        [Required]
        public string PatientName     { get; set; } = string.Empty;

        [Required]
        public string PackageName     { get; set; } = string.Empty;

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public string AppointmentTime { get; set; } = string.Empty;

        public int? DoctorId          { get; set; }
        public string Status          { get; set; } = "Pending";

        // Sample tracking pipeline
        [StringLength(30)]
        public string? SampleId       { get; set; }  // e.g. KSP-20260504-0001

        [StringLength(100)]
        public string? PhlebotomistName { get; set; }

        public DateTime? SampleCollectedAt { get; set; }

        public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;
    }
}
