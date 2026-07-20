namespace HospitalApp.Models
{
    /// <summary>
    /// Represents a patient registered in the hospital system.
    /// </summary>
    public class Patient
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;   // Male / Female / Other

        public string Disease { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public Patient() { }

        public Patient(string fullName, int age, string gender, string disease, string phoneNumber)
        {
            FullName = fullName;
            Age = age;
            Gender = gender;
            Disease = disease;
            PhoneNumber = phoneNumber;
            AdmissionDate = DateTime.UtcNow;
            IsActive = true;
        }

        public void Deactivate() => IsActive = false;
    }
}
