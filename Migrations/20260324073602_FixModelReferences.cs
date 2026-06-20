using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ksp.care.Migrations
{
    /// <inheritdoc />
    public partial class FixModelReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabReports");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "PatientRecords",
                newName: "State");

            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "PatientRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BookingDate",
                table: "PatientRecords",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "PatientRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "PatientRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "PatientRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "PatientRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "PatientRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.CreateTable(
                name: "StaffAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StaffId = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAccounts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StaffAccounts");

            migrationBuilder.DropColumn(
                name: "Age",
                table: "PatientRecords");

            migrationBuilder.DropColumn(
                name: "BookingDate",
                table: "PatientRecords");

            migrationBuilder.DropColumn(
                name: "City",
                table: "PatientRecords");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "PatientRecords");

            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "PatientRecords");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "PatientRecords");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "PatientRecords");

            migrationBuilder.DropColumn(
                name: "Pincode",
                table: "PatientRecords");

            migrationBuilder.DropColumn(
                name: "Place",
                table: "PatientRecords");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "PatientRecords",
                newName: "Name");

            migrationBuilder.CreateTable(
                name: "LabReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PatientId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TestName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabReports", x => x.Id);
                });
        }
    }
}
