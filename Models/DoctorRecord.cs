using System;
using System.ComponentModel.DataAnnotations;

namespace ksp.care.Models
{
    public class DoctorRecord
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Phone, StringLength(15)]
        public string Mobile { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
