using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ksp.care.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientIdToBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "Billings",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "Billings");
        }
    }
}
