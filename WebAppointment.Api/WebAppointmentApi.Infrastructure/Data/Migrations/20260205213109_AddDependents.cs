using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppointmentApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDependents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DependentId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Dependents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuardianUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TcKimlikNo = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dependents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dependents_Users_GuardianUserId",
                        column: x => x.GuardianUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DependentId",
                table: "Appointments",
                column: "DependentId");

            migrationBuilder.CreateIndex(
                name: "IX_Dependents_GuardianUserId",
                table: "Dependents",
                column: "GuardianUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Dependents_TenantId_GuardianUserId_TcKimlikNo",
                table: "Dependents",
                columns: new[] { "TenantId", "GuardianUserId", "TcKimlikNo" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Dependents_DependentId",
                table: "Appointments",
                column: "DependentId",
                principalTable: "Dependents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Dependents_DependentId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "Dependents");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_DependentId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "DependentId",
                table: "Appointments");
        }
    }
}
