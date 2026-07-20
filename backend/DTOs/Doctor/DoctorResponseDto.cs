namespace HospitalApp.DTOs.Doctor
{
    /// <summary>Read-only DTO returned for doctor queries.</summary>
    public class DoctorResponseDto
    {
        public int Id { get; set; }
        public string BadgeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string PayrollPosition { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public int TotalAppointments { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
    }
}
