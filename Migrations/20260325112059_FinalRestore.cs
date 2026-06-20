using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ksp.care.Migrations
{
    /// <inheritdoc />
    public partial class FinalRestore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pincode",
                table: "PatientRecords");

            migrationBuilder.DropColumn(
                name: "Place",
                table: "PatientRecords");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Pincode",
                table: "PatientRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Place",
                table: "PatientRecords",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
