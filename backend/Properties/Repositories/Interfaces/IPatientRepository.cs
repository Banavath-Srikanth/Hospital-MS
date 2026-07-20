using HospitalApp.Models;

namespace HospitalApp.Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetAllAsync();

        Task<Patient?> GetByIdAsync(int id);

        Task<IEnumerable<Patient>> GetByDiseaseAsync(string disease);

        Task<IEnumerable<Patient>> SearchByNameAsync(string name);

        Task<Patient> AddAsync(Patient patient);

        Task<Patient?> UpdateAsync(Patient patient);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}