using HospitalApp.Data;
using HospitalApp.Models;
using HospitalApp.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.Repositories
{
    /// <summary>
    /// Task 2 — PatientSpRepository
    /// All data access is performed through SQL Server Stored Procedures
    /// using EF Core's FromSqlRaw / ExecuteSqlRawAsync APIs.
    ///
    /// Stored Procedures used:
    ///   - sp_SP_GetAllPatients
    ///   - sp_SP_GetPatientById
    ///   - sp_SP_CreatePatient
    ///   - sp_SP_UpdatePatient
    ///   - sp_SP_DeletePatient
    /// </summary>
    public class PatientSpRepository : IPatientSpRepository
    {
        private readonly HospitalDbContext _context;

        public PatientSpRepository(HospitalDbContext context)
        {
            _context = context;
        }

        // ─── READ ALL ─────────────────────────────────────────────────────

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            // FromSqlRaw maps SP result columns to Patient entity properties
            return await _context.Patients
                .FromSqlRaw("EXEC sp_SP_GetAllPatients")
                .AsNoTracking()
                .ToListAsync();
        }

        // ─── READ ONE ─────────────────────────────────────────────────────

        public async Task<Patient?> GetByIdAsync(int id)
        {
            var param = new SqlParameter("@PatientId", id);

            return await _context.Patients
                .FromSqlRaw("EXEC sp_SP_GetPatientById @PatientId", param)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        // ─── CREATE ───────────────────────────────────────────────────────

        public async Task<int> CreateAsync(Patient patient)
        {
            // sp_SP_CreatePatient inserts a row and returns the new ID via OUTPUT
            var newIdParam = new SqlParameter
            {
                ParameterName = "@NewId",
                SqlDbType     = System.Data.SqlDbType.Int,
                Direction     = System.Data.ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_SP_CreatePatient @FullName, @Age, @Gender, @Disease, @PhoneNumber, @NewId OUTPUT",
                new SqlParameter("@FullName",    patient.FullName),
                new SqlParameter("@Age",         patient.Age),
                new SqlParameter("@Gender",      patient.Gender),
                new SqlParameter("@Disease",     patient.Disease),
                new SqlParameter("@PhoneNumber", patient.PhoneNumber),
                newIdParam
            );

            return (int)newIdParam.Value;
        }

        // ─── UPDATE ───────────────────────────────────────────────────────

        public async Task<bool> UpdateAsync(Patient patient)
        {
            var rows = await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_SP_UpdatePatient @PatientId, @FullName, @Age, @Gender, @Disease, @PhoneNumber, @IsActive",
                new SqlParameter("@PatientId",   patient.Id),
                new SqlParameter("@FullName",    patient.FullName),
                new SqlParameter("@Age",         patient.Age),
                new SqlParameter("@Gender",      patient.Gender),
                new SqlParameter("@Disease",     patient.Disease),
                new SqlParameter("@PhoneNumber", patient.PhoneNumber),
                new SqlParameter("@IsActive",    patient.IsActive)
            );

            return rows > 0;
        }

        // ─── DELETE (SOFT) ────────────────────────────────────────────────

        public async Task<bool> DeleteAsync(int id)
        {
            var rows = await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_SP_DeletePatient @PatientId",
                new SqlParameter("@PatientId", id)
            );

            return rows > 0;
        }
    }
}
