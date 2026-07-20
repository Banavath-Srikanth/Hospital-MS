using HospitalApp.DTOs.Patient;
using HospitalApp.Models;
using HospitalApp.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.Controllers
{
    /// <summary>
    /// Task 2 — CRUD via Stored Procedures
    ///
    /// This controller uses PatientSpRepository which calls SQL Server Stored Procedures
    /// instead of EF Core LINQ queries. It mirrors PatientsController but routes
    /// through the SP data access layer.
    ///
    /// Endpoints:
    ///   GET    /api/patients-sp              → sp_SP_GetAllPatients
    ///   GET    /api/patients-sp/{id}         → sp_SP_GetPatientById
    ///   POST   /api/patients-sp              → sp_SP_CreatePatient
    ///   PUT    /api/patients-sp/{id}         → sp_SP_UpdatePatient
    ///   DELETE /api/patients-sp/{id}         → sp_SP_DeletePatient
    /// </summary>
    [ApiController]
    [Route("api/patients-sp")]
    [Produces("application/json")]
    [Authorize]
    public class PatientsSpController : ControllerBase
    {
        private readonly IPatientSpRepository _spRepo;
        private readonly ILogger<PatientsSpController> _logger;

        public PatientsSpController(IPatientSpRepository spRepo, ILogger<PatientsSpController> logger)
        {
            _spRepo = spRepo;
            _logger = logger;
        }

        // ─── GET /api/patients-sp ─────────────────────────────────────────

        /// <summary>Returns all patients via sp_SP_GetAllPatients.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("[SP] Fetching all patients via stored procedure");
            var patients = await _spRepo.GetAllAsync();
            var dtos = patients.Select(MapToDto);
            return Ok(new { success = true, count = dtos.Count(), data = dtos, source = "StoredProcedure" });
        }

        // ─── GET /api/patients-sp/{id} ────────────────────────────────────

        /// <summary>Returns a single patient via sp_SP_GetPatientById.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("[SP] Fetching patient {Id} via stored procedure", id);
            var patient = await _spRepo.GetByIdAsync(id);
            if (patient == null)
                return NotFound(new { success = false, message = $"Patient {id} not found." });

            return Ok(new { success = true, data = MapToDto(patient), source = "StoredProcedure" });
        }

        // ─── POST /api/patients-sp ────────────────────────────────────────

        /// <summary>Creates a patient via sp_SP_CreatePatient.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("[SP] Creating patient via stored procedure: {Name}", dto.FullName);

            var patient = new Patient
            {
                FullName      = dto.FullName.Trim(),
                Age           = dto.Age,
                Gender        = dto.Gender.Trim(),
                Disease       = dto.Disease.Trim(),
                PhoneNumber   = dto.PhoneNumber.Trim(),
                AdmissionDate = DateTime.UtcNow,
                IsActive      = true
            };

            var newId = await _spRepo.CreateAsync(patient);

            return CreatedAtAction(nameof(GetById), new { id = newId },
                new { success = true, message = "Patient created via stored procedure.", newId, source = "StoredProcedure" });
        }

        // ─── PUT /api/patients-sp/{id} ────────────────────────────────────

        /// <summary>Updates a patient via sp_SP_UpdatePatient.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("[SP] Updating patient {Id} via stored procedure", id);

            var patient = new Patient
            {
                Id          = id,
                FullName    = dto.FullName.Trim(),
                Age         = dto.Age,
                Gender      = dto.Gender.Trim(),
                Disease     = dto.Disease.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                IsActive    = dto.IsActive
            };

            var ok = await _spRepo.UpdateAsync(patient);
            if (!ok)
                return NotFound(new { success = false, message = $"Patient {id} not found." });

            return Ok(new { success = true, message = "Patient updated via stored procedure.", source = "StoredProcedure" });
        }

        // ─── DELETE /api/patients-sp/{id} ─────────────────────────────────

        /// <summary>Soft-deletes a patient via sp_SP_DeletePatient.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("[SP] Soft-deleting patient {Id} via stored procedure", id);
            var ok = await _spRepo.DeleteAsync(id);
            if (!ok)
                return NotFound(new { success = false, message = $"Patient {id} not found." });

            return Ok(new { success = true, message = "Patient deactivated via stored procedure.", source = "StoredProcedure" });
        }

        // ─── Mapper ───────────────────────────────────────────────────────

        private static PatientResponseDto MapToDto(Patient p) => new()
        {
            Id                 = p.Id,
            FullName           = p.FullName,
            Age                = p.Age,
            Gender             = p.Gender,
            Disease            = p.Disease,
            PhoneNumber        = p.PhoneNumber,
            AdmissionDate      = p.AdmissionDate,
            IsActive           = p.IsActive,
            TotalAppointments  = p.Appointments?.Count ?? 0
        };
    }
}
