using HospitalApp.Data;
using HospitalApp.Models;
using HospitalApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly HospitalDbContext _context;

        public PatientRepository(HospitalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            // Returns ALL patients (active + inactive).
            // The frontend "Active Only" toggle handles display-level filtering.
            return await (
                from p in _context.Patients.Include(p => p.Appointments)
                orderby p.FullName
                select p
            ).ToListAsync();
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await (
                from p in _context.Patients.Include(p => p.Appointments)
                where p.Id == id
                select p
            ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Patient>> GetByDiseaseAsync(string disease)
        {
            return await (
                from p in _context.Patients.Include(p => p.Appointments)
                where p.IsActive && EF.Functions.Like(p.Disease, $"%{disease}%")
                orderby p.FullName
                select p
            ).ToListAsync();
        }

        public async Task<IEnumerable<Patient>> SearchByNameAsync(string name)
        {
            return await (
                from p in _context.Patients.Include(p => p.Appointments)
                where p.IsActive && EF.Functions.Like(p.FullName, $"%{name}%")
                orderby p.FullName
                select p
            ).ToListAsync();
        }

        public async Task<Patient> AddAsync(Patient patient)
        {
            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<Patient?> UpdateAsync(Patient patient)
        {
            var existing = await _context.Patients.FindAsync(patient.Id);
            if (existing == null) return null;

            existing.FullName = patient.FullName;
            existing.Age = patient.Age;
            existing.Gender = patient.Gender;
            existing.Disease = patient.Disease;
            existing.PhoneNumber = patient.PhoneNumber;
            existing.IsActive = patient.IsActive;

            await _context.SaveChangesAsync();
            return existing;
        }

        // Soft delete: sets IsActive = false to preserve referential integrity with appointments.
        public async Task<bool> DeleteAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return false;

            patient.Deactivate();
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
            => await _context.Patients.AnyAsync(p => p.Id == id);
    }
}
