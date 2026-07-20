using System.ComponentModel.DataAnnotations;

namespace HospitalApp.Models
{
    /// <summary>
    /// Represents a registered user who can log in to the Hospital Management System.
    /// Roles: Admin (full access), Staff (internal), Patient (self-service portal).
    /// </summary>
    public class AppUser
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Role { get; set; } = "Patient";   // Admin | Staff | Patient

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Patient link (only set when Role == "Patient") ────────────────────
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }
    }
}
