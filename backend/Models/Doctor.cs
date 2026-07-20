namespace HospitalApp.Models
{
    /// <summary>
    /// Represents a doctor (employee) registered in the hospital system.
    /// Extended with BadgeId (auto-generated), PayrollPosition, and Department FK.
    /// </summary>
    public class Doctor
    {
        public int Id { get; set; }

        /// <summary>Auto-generated unique badge number, e.g. "EMP-0001"</summary>
        public string BadgeId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;

        /// <summary>e.g. "Senior Consultant", "Junior Resident"</summary>
        public string PayrollPosition { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public bool IsAvailable { get; set; } = true;

        // ── Department FK ──────────────────────────────────────────────────
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        // Navigation property
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public Doctor() { }

        public Doctor(string fullName, string specialization, string phoneNumber, string email,
                      string payrollPosition = "", int? departmentId = null)
        {
            FullName = fullName;
            Specialization = specialization;
            PhoneNumber = phoneNumber;
            Email = email;
            PayrollPosition = payrollPosition;
            DepartmentId = departmentId;
            IsAvailable = true;
        }

        public void MarkUnavailable() => IsAvailable = false;
        public void MarkAvailable() => IsAvailable = true;
    }
}
