using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppointmentApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorProfileApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExperienceSummary",
                table: "Doctors",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GraduationUniversity",
                table: "Doctors",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProfileApprovedAtUtc",
                table: "Doctors",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProfileApprovedByUserId",
                table: "Doctors",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfileStatus",
                table: "Doctors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProfileSubmittedAtUtc",
                table: "Doctors",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExperienceSummary",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "GraduationUniversity",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ProfileApprovedAtUtc",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ProfileApprovedByUserId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ProfileStatus",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ProfileSubmittedAtUtc",
                table: "Doctors");
        }
    }
}
