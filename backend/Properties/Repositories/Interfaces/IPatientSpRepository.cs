using HospitalApp.Models;

namespace HospitalApp.Repositories.Interfaces
{
    /// <summary>
    /// Task 2 — Data access layer that calls SQL Server Stored Procedures
    /// instead of EF Core LINQ queries.
    /// </summary>
    public interface IPatientSpRepository
    {
        /// <summary>Calls sp_SP_GetAllPatients</summary>
        Task<IEnumerable<Patient>> GetAllAsync();

        /// <summary>Calls sp_SP_GetPatientById</summary>
        Task<Patient?> GetByIdAsync(int id);

        /// <summary>Calls sp_SP_CreatePatient — returns new patient ID</summary>
        Task<int> CreateAsync(Patient patient);

        /// <summary>Calls sp_SP_UpdatePatient</summary>
        Task<bool> UpdateAsync(Patient patient);

        /// <summary>Calls sp_SP_DeletePatient (soft delete)</summary>
        Task<bool> DeleteAsync(int id);
    }
}
