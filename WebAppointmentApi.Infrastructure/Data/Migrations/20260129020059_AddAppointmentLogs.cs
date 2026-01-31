using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppointmentApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CreatedAtUtc_DoctorId",
                table: "Appointments",
                columns: new[] { "CreatedAtUtc", "DoctorId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_CreatedAtUtc_DoctorId",
                table: "Appointments");
        }
    }
}
