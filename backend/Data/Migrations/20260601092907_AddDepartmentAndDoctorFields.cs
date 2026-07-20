using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HospitalApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentAndDoctorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BadgeId",
                table: "Doctors",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                table: "Doctors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayrollPosition",
                table: "Doctors",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "CARD", "Cardiology" },
                    { 2, "NEUR", "Neurology" },
                    { 3, "ORTH", "Orthopedics" },
                    { 4, "GEN", "General" }
                });

            // Assign unique BadgeIds to ALL existing doctors (handles any extra rows
            // added outside of seed data) before creating the unique index.
            migrationBuilder.Sql(@"
UPDATE Doctors
SET    BadgeId = 'EMP-' + RIGHT('0000' + CAST(Id AS NVARCHAR(4)), 4)
WHERE  BadgeId = '' OR BadgeId IS NULL;");

            // Assign known DepartmentId + PayrollPosition only for the three seeded rows
            migrationBuilder.UpdateData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DepartmentId", "PayrollPosition" },
                values: new object[] { 1, "Senior Consultant" });

            migrationBuilder.UpdateData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DepartmentId", "PayrollPosition" },
                values: new object[] { 2, "Consultant" });

            migrationBuilder.UpdateData(
                table: "Doctors",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DepartmentId", "PayrollPosition" },
                values: new object[] { 3, "Junior Resident" });

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_BadgeId",
                table: "Doctors",
                column: "BadgeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_DepartmentId",
                table: "Doctors",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Departments_DepartmentId",
                table: "Doctors",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // ── Task 2: Create Stored Procedures ──────────────────────────────

            // sp_SP_GetAllPatients — returns all active patients
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_SP_GetAllPatients
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, FullName, Age, Gender, Disease, PhoneNumber, AdmissionDate, IsActive
    FROM   Patients
    ORDER  BY FullName;
END");

            // sp_SP_GetPatientById — returns one patient by ID
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_SP_GetPatientById
    @PatientId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, FullName, Age, Gender, Disease, PhoneNumber, AdmissionDate, IsActive
    FROM   Patients
    WHERE  Id = @PatientId;
END");

            // sp_SP_CreatePatient — inserts a new patient, returns new ID via OUTPUT
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_SP_CreatePatient
    @FullName    NVARCHAR(200),
    @Age         INT,
    @Gender      NVARCHAR(10),
    @Disease     NVARCHAR(200),
    @PhoneNumber NVARCHAR(15),
    @NewId       INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Patients (FullName, Age, Gender, Disease, PhoneNumber, AdmissionDate, IsActive)
    VALUES (@FullName, @Age, @Gender, @Disease, @PhoneNumber, GETUTCDATE(), 1);
    SET @NewId = SCOPE_IDENTITY();
END");

            // sp_SP_UpdatePatient — updates an existing patient record
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_SP_UpdatePatient
    @PatientId   INT,
    @FullName    NVARCHAR(200),
    @Age         INT,
    @Gender      NVARCHAR(10),
    @Disease     NVARCHAR(200),
    @PhoneNumber NVARCHAR(15),
    @IsActive    BIT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Patients
    SET    FullName    = @FullName,
           Age         = @Age,
           Gender      = @Gender,
           Disease     = @Disease,
           PhoneNumber = @PhoneNumber,
           IsActive    = @IsActive
    WHERE  Id = @PatientId;
END");

            // sp_SP_DeletePatient — soft delete (sets IsActive = 0)
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_SP_DeletePatient
    @PatientId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Patients
    SET    IsActive = 0
    WHERE  Id = @PatientId;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop Stored Procedures
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_SP_DeletePatient");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_SP_UpdatePatient");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_SP_CreatePatient");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_SP_GetPatientById");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_SP_GetAllPatients");

            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Departments_DepartmentId",
                table: "Doctors");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_BadgeId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_DepartmentId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "BadgeId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "PayrollPosition",
                table: "Doctors");
        }
    }
}
