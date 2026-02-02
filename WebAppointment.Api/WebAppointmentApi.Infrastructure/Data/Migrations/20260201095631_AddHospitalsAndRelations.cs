using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppointmentApi.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHospitalsAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HospitalId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HospitalId",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Hospitals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hospitals", x => x.Id);
                });

            // Seed a default hospital and backfill existing departments
            migrationBuilder.Sql(@"SET IDENTITY_INSERT [Hospitals] ON; INSERT INTO [Hospitals] ([Id],[Name],[Address],[Latitude],[Longitude],[IsDeleted]) VALUES (1,'Default Hospital',NULL,NULL,NULL,0); SET IDENTITY_INSERT [Hospitals] OFF;");
            migrationBuilder.Sql(@"UPDATE [Departments] SET [HospitalId] = 1 WHERE [HospitalId] = 0;");

            migrationBuilder.CreateIndex(
                name: "IX_Users_HospitalId",
                table: "Users",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HospitalId_Name",
                table: "Departments",
                columns: new[] { "HospitalId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Hospitals_Latitude_Longitude",
                table: "Hospitals",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Hospitals_HospitalId",
                table: "Departments",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Hospitals_HospitalId",
                table: "Users",
                column: "HospitalId",
                principalTable: "Hospitals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Hospitals_HospitalId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Hospitals_HospitalId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Hospitals");

            migrationBuilder.DropIndex(
                name: "IX_Users_HospitalId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Departments_HospitalId_Name",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "HospitalId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "HospitalId",
                table: "Departments");
        }
    }
}
