using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppointmentApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentCancelReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAtUtc",
                table: "Appointments",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LoginLockouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    FirstFailedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastFailedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LockedUntilUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastIpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginLockouts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoginLockouts_TenantId_LockedUntilUtc",
                table: "LoginLockouts",
                columns: new[] { "TenantId", "LockedUntilUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginLockouts_TenantId_NormalizedEmail",
                table: "LoginLockouts",
                columns: new[] { "TenantId", "NormalizedEmail" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoginLockouts");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Appointments");
        }
    }
}
