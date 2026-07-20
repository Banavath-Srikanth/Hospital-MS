using HospitalApp.DTOs.Doctor;
using HospitalApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.Controllers
{
    /// <summary>
    /// REST API Controller for Doctor management.
    ///
    /// Endpoints:
    ///   GET    /api/doctors                          → List all doctors
    ///   GET    /api/doctors/{id}                     → Get one doctor by ID
    ///   GET    /api/doctors/available                → Get available doctors (LINQ filter)
    ///   GET    /api/doctors/specialization?name=     → Filter by specialization (LINQ)
    ///   GET    /api/doctors/department/{id}          → Filter by department ID
    ///   POST   /api/doctors                          → Register new doctor (BadgeId auto-generated)
    ///   PUT    /api/doctors/{id}                     → Update doctor
    ///   DELETE /api/doctors/{id}                     → Remove doctor
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]   // All endpoints require authentication
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(IDoctorService doctorService, ILogger<DoctorsController> logger)
        {
            _doctorService = doctorService;
            _logger = logger;
        }

        // ─── GET /api/doctors ─────────────────────────────────────────────

        /// <summary>Returns all registered doctors alphabetically.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DoctorResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all doctors");
            var doctors = await _doctorService.GetAllDoctorsAsync();
            return Ok(new { success = true, count = doctors.Count(), data = doctors });
        }

        // ─── GET /api/doctors/{id} ────────────────────────────────────────

        /// <summary>Returns a single doctor by their ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(DoctorResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null)
                return NotFound(new { success = false, message = $"Doctor with ID {id} not found." });

            return Ok(new { success = true, data = doctor });
        }

        // ─── GET /api/doctors/available ───────────────────────────────────

        /// <summary>Returns only available doctors (LINQ boolean filter).</summary>
        [HttpGet("available")]
        [ProducesResponseType(typeof(IEnumerable<DoctorResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailable()
        {
            _logger.LogInformation("Fetching available doctors");
            var doctors = await _doctorService.GetAvailableDoctorsAsync();
            return Ok(new { success = true, count = doctors.Count(), data = doctors });
        }

        // ─── GET /api/doctors/specialization?name= ────────────────────────

        /// <summary>Filters doctors by specialization keyword (LINQ partial match).</summary>
        [HttpGet("specialization")]
        [ProducesResponseType(typeof(IEnumerable<DoctorResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBySpecialization([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { success = false, message = "Specialization name cannot be empty." });

            _logger.LogInformation("Filtering doctors by specialization: {Spec}", name);
            var doctors = await _doctorService.GetDoctorsBySpecializationAsync(name);
            return Ok(new { success = true, count = doctors.Count(), data = doctors });
        }

        // ─── GET /api/doctors/department/{id} ────────────────────────────

        /// <summary>Returns all doctors belonging to a specific department.</summary>
        [HttpGet("department/{departmentId:int}")]
        [ProducesResponseType(typeof(IEnumerable<DoctorResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByDepartment(int departmentId)
        {
            _logger.LogInformation("Filtering doctors by department ID: {DeptId}", departmentId);
            var doctors = await _doctorService.GetDoctorsByDepartmentAsync(departmentId);
            return Ok(new { success = true, count = doctors.Count(), data = doctors });
        }

        // ─── POST /api/doctors ────────────────────────────────────────────

        /// <summary>Registers a new doctor. Email must be unique. BadgeId is auto-generated. Admin only.</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DoctorResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateDoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Registering new doctor: {Name}", dto.FullName);
                var created = await _doctorService.CreateDoctorAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id },
                    new { success = true, message = "Doctor registered successfully.", data = created });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        // ─── PUT /api/doctors/{id} ────────────────────────────────────────

        /// <summary>Updates an existing doctor's information. Admin only.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DoctorResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _doctorService.UpdateDoctorAsync(id, dto);
                if (updated == null)
                    return NotFound(new { success = false, message = $"Doctor with ID {id} not found." });

                return Ok(new { success = true, message = "Doctor updated successfully.", data = updated });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { success = false, message = ex.Message });
            }
        }

        // ─── DELETE /api/doctors/{id} ─────────────────────────────────────

        /// <summary>Removes a doctor from the system (hard delete). Admin only.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting doctor with ID {Id}", id);
            var result = await _doctorService.DeleteDoctorAsync(id);

            if (!result)
                return NotFound(new { success = false, message = $"Doctor with ID {id} not found." });

            return Ok(new { success = true, message = $"Doctor {id} removed successfully." });
        }
    }
}
