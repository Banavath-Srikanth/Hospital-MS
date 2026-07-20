using System.ComponentModel.DataAnnotations;

namespace HospitalApp.DTOs.Patient
{
    /// <summary>DTO used when creating a new patient (POST /api/patients).</summary>
    public class CreatePatientDto
    {
        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Range(1, 130)]
        public int Age { get; set; }

        [Required]
        [RegularExpression("Male|Female|Other", ErrorMessage = "Gender must be Male, Female, or Other")]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Disease { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;
        
    }
}
