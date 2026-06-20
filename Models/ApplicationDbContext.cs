using Microsoft.EntityFrameworkCore;
using ksp.care.Models;

namespace ksp.care
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<PatientRecord>      PatientRecords { get; set; }
        public DbSet<AppointmentRecord>  Appointments   { get; set; }
        public DbSet<ReportRecord>       Reports        { get; set; }
        public DbSet<BillingRecord>      Billings       { get; set; }
        public DbSet<DoctorRecord>       Doctors        { get; set; }
        public DbSet<PrescriptionRecord> Prescriptions  { get; set; }
        public DbSet<TestRecord>         Tests          { get; set; }
        public DbSet<ContactMessage>     ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PatientRecord>()
                .HasIndex(p => p.Mobile)
                .IsUnique();
        }
    }
}
