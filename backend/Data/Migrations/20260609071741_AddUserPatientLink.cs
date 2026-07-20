using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPatientLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PatientId",
                table: "AppUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_PatientId",
                table: "AppUsers",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppUsers_Patients_PatientId",
                table: "AppUsers",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppUsers_Patients_PatientId",
                table: "AppUsers");

            migrationBuilder.DropIndex(
                name: "IX_AppUsers_PatientId",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "AppUsers");
        }
    }
}
