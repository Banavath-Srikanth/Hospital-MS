namespace HospitalApp.Models
{
    /// <summary>
    /// Represents a hospital department (e.g. Cardiology, Neurology).
    /// Doctors are assigned to a department via FK.
    /// </summary>
    public class Department
    {
        public int Id { get; set; }

        /// <summary>e.g. "Cardiology"</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Short code displayed on badges, e.g. "CARD"</summary>
        public string Code { get; set; } = string.Empty;

        // Navigation
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
