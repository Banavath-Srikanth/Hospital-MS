using HospitalApp.DTOs.Patient;
using HospitalApp.Models;
using HospitalApp.Repositories.Interfaces;
using HospitalApp.Services.Interfaces;

namespace HospitalApp.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _repository;

        public PatientService(IPatientRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PatientResponseDto>> GetAllPatientsAsync()
        {
            var patients = await _repository.GetAllAsync();
            return from p in patients select MapToResponseDto(p);
        }

        public async Task<PatientResponseDto?> GetPatientByIdAsync(int id)
        {
            var patient = await _repository.GetByIdAsync(id);
            return patient == null ? null : MapToResponseDto(patient);
        }

        public async Task<IEnumerable<PatientResponseDto>> GetPatientsByDiseaseAsync(string disease)
        {
            var patients = await _repository.GetByDiseaseAsync(disease);
            return from p in patients select MapToResponseDto(p);
        }

        public async Task<IEnumerable<PatientResponseDto>> SearchPatientsByNameAsync(string name)
        {
            var patients = await _repository.SearchByNameAsync(name);
            return from p in patients select MapToResponseDto(p);
        }

        public async Task<PatientResponseDto> CreatePatientAsync(CreatePatientDto dto)
        {
            var patient = new Patient
            {
                FullName = dto.FullName.Trim(),
                Age = dto.Age,
                Gender = dto.Gender,
                Disease = dto.Disease.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                AdmissionDate = DateTime.UtcNow,
                IsActive = true
            };

            var created = await _repository.AddAsync(patient);
            return MapToResponseDto(created);
        }

        public async Task<PatientResponseDto?> UpdatePatientAsync(int id, UpdatePatientDto dto)
        {
            if (!await _repository.ExistsAsync(id)) return null;

            var patient = new Patient
            {
                Id = id,
                FullName = dto.FullName.Trim(),
                Age = dto.Age,
                Gender = dto.Gender,
                Disease = dto.Disease.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                IsActive = dto.IsActive
            };

            var updated = await _repository.UpdateAsync(patient);
            return updated == null ? null : MapToResponseDto(updated);
        }

        public async Task<bool> DeletePatientAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static PatientResponseDto MapToResponseDto(Patient patient)
        {
            return new PatientResponseDto
            {
                Id = patient.Id,
                FullName = patient.FullName,
                Age = patient.Age,
                Gender = patient.Gender,
                Disease = patient.Disease,
                PhoneNumber = patient.PhoneNumber,
                AdmissionDate = patient.AdmissionDate,
                IsActive = patient.IsActive,
                TotalAppointments = patient.Appointments?.Count ?? 0
            };
        }
    }
}
