using HospitalApp.Models;

namespace HospitalApp.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<IEnumerable<Doctor>> GetAllAsync();

        Task<Doctor?> GetByIdAsync(int id);

        Task<IEnumerable<Doctor>> GetBySpecializationAsync(string specialization);

        Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync();

        Task<IEnumerable<Doctor>> GetByDepartmentAsync(int departmentId);

        Task<Doctor> AddAsync(Doctor doctor);

        Task<Doctor?> UpdateAsync(Doctor doctor);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);

        Task<bool> EmailExistsAsync(string email, int? excludeId = null);

        /// <summary>Returns the next sequential badge number string, e.g. "EMP-0004"</summary>
        Task<string> GenerateNextBadgeIdAsync();
    }
}