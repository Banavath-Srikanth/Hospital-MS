using HospitalApp.DTOs.Doctor;
using HospitalApp.Models;
using HospitalApp.Repositories.Interfaces;
using HospitalApp.Services.Interfaces;

namespace HospitalApp.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _repository;

        public DoctorService(IDoctorRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync()
        {
            var doctors = await _repository.GetAllAsync();
            return from d in doctors select MapToResponseDto(d);
        }

        public async Task<DoctorResponseDto?> GetDoctorByIdAsync(int id)
        {
            var doctor = await _repository.GetByIdAsync(id);
            return doctor == null ? null : MapToResponseDto(doctor);
        }

        public async Task<IEnumerable<DoctorResponseDto>> GetDoctorsBySpecializationAsync(string specialization)
        {
            var doctors = await _repository.GetBySpecializationAsync(specialization);
            return from d in doctors select MapToResponseDto(d);
        }

        public async Task<IEnumerable<DoctorResponseDto>> GetAvailableDoctorsAsync()
        {
            var doctors = await _repository.GetAvailableDoctorsAsync();
            return from d in doctors select MapToResponseDto(d);
        }

        public async Task<IEnumerable<DoctorResponseDto>> GetDoctorsByDepartmentAsync(int departmentId)
        {
            var doctors = await _repository.GetByDepartmentAsync(departmentId);
            return from d in doctors select MapToResponseDto(d);
        }

        public async Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorDto dto)
        {
            if (await _repository.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException($"A doctor with email '{dto.Email}' already exists.");

            // Auto-generate badge ID
            var badgeId = await _repository.GenerateNextBadgeIdAsync();

            var doctor = new Doctor
            {
                BadgeId         = badgeId,
                FullName        = dto.FullName.Trim(),
                Specialization  = dto.Specialization.Trim(),
                PayrollPosition = dto.PayrollPosition.Trim(),
                PhoneNumber     = dto.PhoneNumber.Trim(),
                Email           = dto.Email.Trim().ToLower(),
                IsAvailable     = true,
                DepartmentId    = dto.DepartmentId
            };

            var created = await _repository.AddAsync(doctor);
            return MapToResponseDto(created);
        }

        public async Task<DoctorResponseDto?> UpdateDoctorAsync(int id, UpdateDoctorDto dto)
        {
            if (!await _repository.ExistsAsync(id)) return null;

            if (await _repository.EmailExistsAsync(dto.Email, excludeId: id))
                throw new InvalidOperationException($"Email '{dto.Email}' is already used by another doctor.");

            var doctor = new Doctor
            {
                Id              = id,
                FullName        = dto.FullName.Trim(),
                Specialization  = dto.Specialization.Trim(),
                PayrollPosition = dto.PayrollPosition.Trim(),
                PhoneNumber     = dto.PhoneNumber.Trim(),
                Email           = dto.Email.Trim().ToLower(),
                IsAvailable     = dto.IsAvailable,
                DepartmentId    = dto.DepartmentId
            };

            var updated = await _repository.UpdateAsync(doctor);
            return updated == null ? null : MapToResponseDto(updated);
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static DoctorResponseDto MapToResponseDto(Doctor doctor)
        {
            return new DoctorResponseDto
            {
                Id                 = doctor.Id,
                BadgeId            = doctor.BadgeId,
                FullName           = doctor.FullName,
                Specialization     = doctor.Specialization,
                PayrollPosition    = doctor.PayrollPosition,
                PhoneNumber        = doctor.PhoneNumber,
                Email              = doctor.Email,
                IsAvailable        = doctor.IsAvailable,
                TotalAppointments  = doctor.Appointments?.Count ?? 0,
                DepartmentId       = doctor.DepartmentId,
                DepartmentName     = doctor.Department?.Name ?? string.Empty
            };
        }
    }
}
