using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ksp.care.Models
{
    public class TestRecord
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string TestName { get; set; } = string.Empty;

        [Required, StringLength(80)]
        public string Category { get; set; } = string.Empty; // Haematology, Biochemistry, etc.

        [StringLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 9999999.99)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
