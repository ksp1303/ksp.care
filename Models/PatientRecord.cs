using System.ComponentModel.DataAnnotations;

namespace ksp.care.Models
{
    public class PatientRecord
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName  { get; set; } = string.Empty;

        [Required, Phone, StringLength(15)]
        public string Mobile    { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Password  { get; set; } = string.Empty;

        [EmailAddress, StringLength(100)]
        public string? Email    { get; set; }

        [Range(0, 150)]
        public int    Age       { get; set; }

        [Required, StringLength(10)]
        public string Gender    { get; set; } = string.Empty;

        [StringLength(50)]
        public string City      { get; set; } = string.Empty;

        [StringLength(10)]
        public string Pincode   { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Place    { get; set; }

        [StringLength(50)]
        public string? State    { get; set; }

        [StringLength(20)]
        public string? Ward     { get; set; }

        [StringLength(255)]
        public string? ProfilePhoto { get; set; }
    }
}
