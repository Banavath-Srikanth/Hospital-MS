using HospitalApp.Data;
using HospitalApp.Models;
using HospitalApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly HospitalDbContext _context;

        public DoctorRepository(HospitalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            return await (
                from d in _context.Doctors
                    .Include(d => d.Appointments)
                    .Include(d => d.Department)
                orderby d.FullName
                select d
            ).ToListAsync();
        }

        public async Task<Doctor?> GetByIdAsync(int id)
        {
            return await (
                from d in _context.Doctors
                    .Include(d => d.Appointments)
                    .Include(d => d.Department)
                where d.Id == id
                select d
            ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Doctor>> GetBySpecializationAsync(string specialization)
        {
            return await (
                from d in _context.Doctors
                    .Include(d => d.Appointments)
                    .Include(d => d.Department)
                where EF.Functions.Like(d.Specialization, $"%{specialization}%")
                orderby d.FullName
                select d
            ).ToListAsync();
        }

        public async Task<IEnumerable<Doctor>> GetAvailableDoctorsAsync()
        {
            return await (
                from d in _context.Doctors
                    .Include(d => d.Appointments)
                    .Include(d => d.Department)
                where d.IsAvailable
                orderby d.Specialization
                select d
            ).ToListAsync();
        }

        public async Task<IEnumerable<Doctor>> GetByDepartmentAsync(int departmentId)
        {
            return await (
                from d in _context.Doctors
                    .Include(d => d.Appointments)
                    .Include(d => d.Department)
                where d.DepartmentId == departmentId
                orderby d.FullName
                select d
            ).ToListAsync();
        }

        public async Task<Doctor> AddAsync(Doctor doctor)
        {
            await _context.Doctors.AddAsync(doctor);
            await _context.SaveChangesAsync();
            // Re-fetch with Department nav loaded
            return (await GetByIdAsync(doctor.Id))!;
        }

        public async Task<Doctor?> UpdateAsync(Doctor doctor)
        {
            var existing = await _context.Doctors.FindAsync(doctor.Id);
            if (existing == null) return null;

            existing.FullName        = doctor.FullName;
            existing.Specialization  = doctor.Specialization;
            existing.PayrollPosition = doctor.PayrollPosition;
            existing.PhoneNumber     = doctor.PhoneNumber;
            existing.Email           = doctor.Email;
            existing.IsAvailable     = doctor.IsAvailable;
            existing.DepartmentId    = doctor.DepartmentId;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(existing.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return false;

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
            => await _context.Doctors.AnyAsync(d => d.Id == id);

        // Excludes current record by ID to allow email updates without false conflicts.
        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            return await _context.Doctors
                .AnyAsync(d => d.Email == email && (excludeId == null || d.Id != excludeId));
        }

        /// <summary>
        /// Generates the next badge ID in the sequence "EMP-XXXX".
        /// Reads the current max numeric suffix and increments by 1.
        /// </summary>
        public async Task<string> GenerateNextBadgeIdAsync()
        {
            var lastBadge = await _context.Doctors
                .OrderByDescending(d => d.Id)
                .Select(d => d.BadgeId)
                .FirstOrDefaultAsync();

            int nextNum = 1;
            if (!string.IsNullOrEmpty(lastBadge) && lastBadge.StartsWith("EMP-"))
            {
                if (int.TryParse(lastBadge[4..], out int parsed))
                    nextNum = parsed + 1;
            }

            return $"EMP-{nextNum:D4}";
        }
    }
}
