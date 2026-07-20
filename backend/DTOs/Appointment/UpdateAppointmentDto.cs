using System.ComponentModel.DataAnnotations;

namespace HospitalApp.DTOs.Appointment
{
    /// <summary>DTO used when updating an appointment (PUT /api/appointments/{id}).</summary>
    public class UpdateAppointmentDto
    {
        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        [RegularExpression("Scheduled|Completed|Cancelled",
            ErrorMessage = "Status must be Scheduled, Completed, or Cancelled")]
        public string Status { get; set; } = string.Empty;
    }
}
