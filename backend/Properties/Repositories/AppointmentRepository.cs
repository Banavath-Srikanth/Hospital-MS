using HospitalApp.Data;
using HospitalApp.Models;
using HospitalApp.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly HospitalDbContext _context;

        public AppointmentRepository(HospitalDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await (
                from a in _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                orderby a.AppointmentDate descending
                select a
            ).ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await (
                from a in _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                where a.Id == id
                select a
            ).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId)
        {
            return await (
                from a in _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                where a.PatientId == patientId
                orderby a.AppointmentDate descending
                select a
            ).ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId)
        {
            return await (
                from a in _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                where a.DoctorId == doctorId
                orderby a.AppointmentDate descending
                select a
            ).ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetByStatusAsync(string status)
        {
            return await (
                from a in _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                where a.Status == status
                orderby a.AppointmentDate descending
                select a
            ).ToListAsync();
        }

        public async Task<Appointment> AddAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
            return (await GetByIdAsync(appointment.Id))!;
        }

        public async Task<Appointment?> UpdateAsync(Appointment appointment)
        {
            var existing = await _context.Appointments.FindAsync(appointment.Id);
            if (existing == null) return null;

            existing.AppointmentDate = appointment.AppointmentDate;
            existing.Status = appointment.Status;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(existing.Id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
            => await _context.Appointments.AnyAsync(a => a.Id == id);
    }
}
