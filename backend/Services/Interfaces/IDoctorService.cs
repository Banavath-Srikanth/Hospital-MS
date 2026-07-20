using HospitalApp.DTOs.Doctor;

namespace HospitalApp.Services.Interfaces
{
    /// <summary>
    /// Service interface for Doctor business logic.
    /// </summary>
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync();
        Task<DoctorResponseDto?> GetDoctorByIdAsync(int id);
        Task<IEnumerable<DoctorResponseDto>> GetDoctorsBySpecializationAsync(string specialization);
        Task<IEnumerable<DoctorResponseDto>> GetAvailableDoctorsAsync();
        Task<IEnumerable<DoctorResponseDto>> GetDoctorsByDepartmentAsync(int departmentId);
        Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorDto dto);
        Task<DoctorResponseDto?> UpdateDoctorAsync(int id, UpdateDoctorDto dto);
        Task<bool> DeleteDoctorAsync(int id);
    }
}
