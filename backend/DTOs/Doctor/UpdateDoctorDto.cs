using System.ComponentModel.DataAnnotations;

namespace HospitalApp.DTOs.Doctor
{
    /// <summary>DTO used when updating a doctor (PUT /api/doctors/{id}).</summary>
    public class UpdateDoctorDto
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(100)]
        public string PayrollPosition { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }

        [Required]
        public int DepartmentId { get; set; }
    }
}
