using HospitalApp.DTOs.Appointment;

namespace HospitalApp.Services.Interfaces
{
    /// <summary>
    /// Service interface for Appointment business logic.
    /// </summary>
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync();
        Task<AppointmentResponseDto?> GetAppointmentByIdAsync(int id);
        Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByPatientAsync(int patientId);
        Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByDoctorAsync(int doctorId);
        Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByStatusAsync(string status);
        Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto);
        Task<AppointmentResponseDto?> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto);
        Task<bool> DeleteAppointmentAsync(int id);
    }
}
