using System.ComponentModel.DataAnnotations;

namespace HospitalApp.DTOs
{
    // ── Register ──────────────────────────────────────────────────────────────
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // ── Patient-specific fields (required when Role == "Patient") ─────────
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Range(1, 120)]
        public int? Age { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; }   // Male | Female | Other

        /// <summary>Role requested: "Patient" (default) or "Staff" (admin-created only).</summary>
        public string Role { get; set; } = "Patient";
    }

    // ── Login ─────────────────────────────────────────────────────────────────
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;
    }

    // ── Response ──────────────────────────────────────────────────────────────
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public int? PatientId { get; set; }
    }
}
