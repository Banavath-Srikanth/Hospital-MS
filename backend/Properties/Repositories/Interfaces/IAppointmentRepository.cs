using HospitalApp.Models;

namespace HospitalApp.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAsync();

        Task<Appointment?> GetByIdAsync(int id);

        Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);

        Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId);

        Task<IEnumerable<Appointment>> GetByStatusAsync(string status);

        Task<Appointment> AddAsync(Appointment appointment);

        Task<Appointment?> UpdateAsync(Appointment appointment);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}