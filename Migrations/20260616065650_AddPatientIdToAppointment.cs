using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ksp.care.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientIdToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "Appointments",
                type: "integer",
                nullable: true);

            // Backfill existing bookings: link each appointment to its patient
            // profile by matching mobile number, so historical bookings are also
            // saved under the stable Patient ID.
            migrationBuilder.Sql(@"
                UPDATE ""Appointments"" a
                SET ""PatientId"" = p.""Id""
                FROM ""PatientRecords"" p
                WHERE a.""PatientMobile"" = p.""Mobile"" AND a.""PatientId"" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "Appointments");
        }
    }
}
