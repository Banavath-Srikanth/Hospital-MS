using HospitalApp.DTOs.Appointment;
using HospitalApp.Models;
using HospitalApp.Repositories.Interfaces;
using HospitalApp.Services.Interfaces;

namespace HospitalApp.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository)
        {
            _appointmentRepository = appointmentRepository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync()
        {
            var appointments = await _appointmentRepository.GetAllAsync();
            return from a in appointments select MapToResponseDto(a);
        }

        public async Task<AppointmentResponseDto?> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            return appointment == null ? null : MapToResponseDto(appointment);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByPatientAsync(int patientId)
        {
            var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId);
            return from a in appointments select MapToResponseDto(a);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByDoctorAsync(int doctorId)
        {
            var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId);
            return from a in appointments select MapToResponseDto(a);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByStatusAsync(string status)
        {
            var appointments = await _appointmentRepository.GetByStatusAsync(status);
            return from a in appointments select MapToResponseDto(a);
        }

        public async Task<AppointmentResponseDto> CreateAppointmentAsync(CreateAppointmentDto dto)
        {
            var patient = await _patientRepository.GetByIdAsync(dto.PatientId);
            if (patient == null || !patient.IsActive)
                throw new InvalidOperationException($"Patient with ID {dto.PatientId} not found or inactive.");

            var doctor = await _doctorRepository.GetByIdAsync(dto.DoctorId);
            if (doctor == null)
                throw new InvalidOperationException($"Doctor with ID {dto.DoctorId} not found.");
            if (!doctor.IsAvailable)
                throw new InvalidOperationException($"Dr. {doctor.FullName} is currently not available.");

            if (dto.AppointmentDate <= DateTime.Now)
                throw new InvalidOperationException("Appointment date must be in the future.");

            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                AppointmentDate = dto.AppointmentDate,
                Status = AppointmentStatus.Scheduled
            };

            var created = await _appointmentRepository.AddAsync(appointment);
            return MapToResponseDto(created);
        }

        public async Task<AppointmentResponseDto?> UpdateAppointmentAsync(int id, UpdateAppointmentDto dto)
        {
            var existing = await _appointmentRepository.GetByIdAsync(id);
            if (existing == null) return null;

            if (existing.Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Cannot update a cancelled appointment.");

            if (existing.Status == AppointmentStatus.Completed)
                throw new InvalidOperationException("Cannot update a completed appointment.");

            if (dto.Status == AppointmentStatus.Scheduled &&
                dto.AppointmentDate <= DateTime.Now)
                throw new InvalidOperationException("Appointment date must be in the future.");

            existing.AppointmentDate = dto.AppointmentDate;
            existing.Status = dto.Status;

            var updated = await _appointmentRepository.UpdateAsync(existing);
            return updated == null ? null : MapToResponseDto(updated);
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            return await _appointmentRepository.DeleteAsync(id);
        }

        private static AppointmentResponseDto MapToResponseDto(Appointment appointment)
        {
            return new AppointmentResponseDto
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient?.FullName ?? "Unknown",
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor?.FullName ?? "Unknown",
                DoctorSpecialization = appointment.Doctor?.Specialization ?? "Unknown",
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status
            };
        }
    }
}
