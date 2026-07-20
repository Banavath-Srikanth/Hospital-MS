namespace HospitalApp.Models
{
    /// <summary>
    /// Represents a scheduled appointment between a Patient and a Doctor.
    /// </summary>
    public class Appointment
    {
        public int Id { get; set; }

        public int PatientId { get; set; }
        public int DoctorId { get; set; }

        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = AppointmentStatus.Scheduled;  // Scheduled / Completed / Cancelled

        // Navigation properties
        public Patient Patient { get; set; } = null!;
        public Doctor Doctor { get; set; } = null!;

        public Appointment() { }

        public Appointment(int patientId, int doctorId, DateTime appointmentDate)
        {
            PatientId = patientId;
            DoctorId = doctorId;
            AppointmentDate = appointmentDate;
            Status = AppointmentStatus.Scheduled;
        }

        public void Complete() => Status = AppointmentStatus.Completed;
        public void Cancel() => Status = AppointmentStatus.Cancelled;
    }

    /// <summary>Static class to hold appointment status constants.</summary>
    public static class AppointmentStatus
    {
        public const string Scheduled = "Scheduled";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }
}
