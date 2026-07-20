using HospitalApp.DTOs.Patient;

namespace HospitalApp.Services.Interfaces
{
    /// <summary>
    /// Service interface for Patient business logic.
    /// Step 2 — Clean Architecture: business rules live here, not in controllers/repositories.
    ///</summary>
    public interface IPatientService
    {
        Task<IEnumerable<PatientResponseDto>> GetAllPatientsAsync();
        Task<PatientResponseDto?> GetPatientByIdAsync(int id);
        Task<IEnumerable<PatientResponseDto>> GetPatientsByDiseaseAsync(string disease);
        Task<IEnumerable<PatientResponseDto>> SearchPatientsByNameAsync(string name);
        Task<PatientResponseDto> CreatePatientAsync(CreatePatientDto dto);
        Task<PatientResponseDto?> UpdatePatientAsync(int id, UpdatePatientDto dto);
        Task<bool> DeletePatientAsync(int id);
    }
}
