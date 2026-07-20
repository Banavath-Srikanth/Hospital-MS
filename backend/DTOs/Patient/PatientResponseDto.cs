namespace HospitalApp.DTOs.Patient
{
    /// <summary>Read-only DTO returned to the caller.</summary>
    public class PatientResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Disease { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public bool IsActive { get; set; }
        public int TotalAppointments { get; set; }
    }
}
