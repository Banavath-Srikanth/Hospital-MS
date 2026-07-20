using HospitalApp.DTOs.Patient;
using HospitalApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.Controllers
{
    /// <summary>
    /// REST API Controller for Patient management.
    /// Step 3 — Web API: exposes CRUD endpoints + LINQ-backed search and filter.
    ///
    /// Endpoints:
    ///   GET    /api/patients                   → List all active patients
    ///   GET    /api/patients/{id}              → Get one patient by ID
    ///   GET    /api/patients/search?name=      → Search by name (LINQ partial match)
    ///   GET    /api/patients/disease?name=     → Filter by disease (LINQ partial match)
    ///   POST   /api/patients                   → Create new patient
    ///   PUT    /api/patients/{id}              → Update existing patient
    ///   DELETE /api/patients/{id}              → Soft-delete patient
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly ILogger<PatientsController> _logger;

        public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
        {
            _patientService = patientService;
            _logger = logger;
        }

        // ─── GET /api/patients ────────────────────────────────────────────

        /// <summary>Returns all active patients sorted alphabetically.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PatientResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all active patients");
            var patients = await _patientService.GetAllPatientsAsync();
            return Ok(new { success = true, count = patients.Count(), data = patients });
        }

        // ─── GET /api/patients/{id} ───────────────────────────────────────

        /// <summary>Returns a single patient by their ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PatientResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Fetching patient with ID {Id}", id);
            var patient = await _patientService.GetPatientByIdAsync(id);

            if (patient == null)
                return NotFound(new { success = false, message = $"Patient with ID {id} not found." });

            return Ok(new { success = true, data = patient });
        }

        // ─── GET /api/patients/search?name= ──────────────────────────────

        /// <summary>Searches patients by partial name match (LINQ query).</summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<PatientResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { success = false, message = "Search name cannot be empty." });

            _logger.LogInformation("Searching patients by name: {Name}", name);
            var patients = await _patientService.SearchPatientsByNameAsync(name);
            return Ok(new { success = true, count = patients.Count(), data = patients });
        }

        // ─── GET /api/patients/disease?name= ─────────────────────────────

        /// <summary>Filters patients by disease keyword (LINQ query).</summary>
        [HttpGet("disease")]
        [ProducesResponseType(typeof(IEnumerable<PatientResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByDisease([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { success = false, message = "Disease name cannot be empty." });

            _logger.LogInformation("Filtering patients by disease: {Disease}", name);
            var patients = await _patientService.GetPatientsByDiseaseAsync(name);
            return Ok(new { success = true, count = patients.Count(), data = patients });
        }

        // ─── POST /api/patients ─────────────────────────────────────────

        /// <summary>Registers a new patient in the hospital system. Admin only.</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(PatientResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreatePatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Creating new patient: {Name}", dto.FullName);
            var created = await _patientService.CreatePatientAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                new { success = true, message = "Patient registered successfully.", data = created });
        }

        // ─── PUT /api/patients/{id} ─────────────────────────────────────────

        /// <summary>Updates an existing patient's information. Admin only.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(PatientResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Updating patient with ID {Id}", id);
            var updated = await _patientService.UpdatePatientAsync(id, dto);

            if (updated == null)
                return NotFound(new { success = false, message = $"Patient with ID {id} not found." });

            return Ok(new { success = true, message = "Patient updated successfully.", data = updated });
        }

        // ─── DELETE /api/patients/{id} ─────────────────────────────────────────

        /// <summary>Soft-deletes a patient (marks as inactive, preserves history). Admin only.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Soft-deleting patient with ID {Id}", id);
            var result = await _patientService.DeletePatientAsync(id);

            if (!result)
                return NotFound(new { success = false, message = $"Patient with ID {id} not found." });

            return Ok(new { success = true, message = $"Patient {id} has been deactivated successfully." });
        }
    }
}
