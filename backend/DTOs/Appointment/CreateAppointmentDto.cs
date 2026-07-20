using System.ComponentModel.DataAnnotations;

namespace HospitalApp.DTOs.Appointment
{
    /// <summary>DTO used when scheduling a new appointment (POST /api/appointments).</summary>
    public class CreateAppointmentDto
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }
    }
}
