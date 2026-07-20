using HospitalApp.DTOs.Appointment;
using HospitalApp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalApp.Controllers
{
    /// <summary>
    /// REST API Controller for Appointment management.
    ///
    /// Endpoints:
    ///   GET    /api/appointments                       → All appointments         [Admin, Staff]
    ///   GET    /api/appointments/my                    → My appointments          [Patient]
    ///   GET    /api/appointments/{id}                  → Single appointment       [Authorized]
    ///   GET    /api/appointments/patient/{patientId}   → By patient               [Admin, Staff]
    ///   GET    /api/appointments/doctor/{doctorId}     → By doctor                [Admin, Staff]
    ///   GET    /api/appointments/status?name=          → By status                [Admin, Staff]
    ///   POST   /api/appointments                       → Schedule new appointment [Authorized]
    ///   PUT    /api/appointments/{id}                  → Update/reschedule        [Admin, Staff]
    ///   DELETE /api/appointments/{id}                  → Cancel/remove            [Admin, Staff]
    ///   PATCH  /api/appointments/{id}/cancel           → Patient cancels own appt [Patient]
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(IAppointmentService appointmentService, ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _logger = logger;
        }

        // ─── GET /api/appointments ────────────────────────────────────────
        /// <summary>Returns all appointments, newest first. Admin/Staff only.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Fetching all appointments");
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            return Ok(new { success = true, count = appointments.Count(), data = appointments });
        }

        // ─── GET /api/appointments/my ─────────────────────────────────────
        /// <summary>Returns the current patient's own appointments.</summary>
        [HttpGet("my")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMyAppointments()
        {
            var patientIdClaim = User.FindFirstValue("patientId");
            if (string.IsNullOrEmpty(patientIdClaim) || !int.TryParse(patientIdClaim, out var patientId))
                return BadRequest(new { success = false, message = "Patient ID not found in token." });

            _logger.LogInformation("Fetching appointments for logged-in patient ID {PatientId}", patientId);
            var appointments = await _appointmentService.GetAppointmentsByPatientAsync(patientId);
            return Ok(new { success = true, count = appointments.Count(), data = appointments });
        }

        // ─── GET /api/appointments/{id} ───────────────────────────────────
        /// <summary>Returns a single appointment by ID.</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound(new { success = false, message = $"Appointment with ID {id} not found." });

            return Ok(new { success = true, data = appointment });
        }

        // ─── GET /api/appointments/patient/{patientId} ────────────────────
        /// <summary>Returns all appointments for a specific patient. Admin/Staff only.</summary>
        [HttpGet("patient/{patientId:int}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByPatient(int patientId)
        {
            _logger.LogInformation("Fetching appointments for patient ID {PatientId}", patientId);
            var appointments = await _appointmentService.GetAppointmentsByPatientAsync(patientId);
            return Ok(new { success = true, count = appointments.Count(), data = appointments });
        }

        // ─── GET /api/appointments/doctor/{doctorId} ──────────────────────
        /// <summary>Returns all appointments for a specific doctor. Admin/Staff only.</summary>
        [HttpGet("doctor/{doctorId:int}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByDoctor(int doctorId)
        {
            _logger.LogInformation("Fetching appointments for doctor ID {DoctorId}", doctorId);
            var appointments = await _appointmentService.GetAppointmentsByDoctorAsync(doctorId);
            return Ok(new { success = true, count = appointments.Count(), data = appointments });
        }

        // ─── GET /api/appointments/status?name= ───────────────────────────
        /// <summary>Filters appointments by status. Admin/Staff only.</summary>
        [HttpGet("status")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByStatus([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { success = false, message = "Status cannot be empty." });

            _logger.LogInformation("Filtering appointments by status: {Status}", name);
            var appointments = await _appointmentService.GetAppointmentsByStatusAsync(name);
            return Ok(new { success = true, count = appointments.Count(), data = appointments });
        }

        // ─── POST /api/appointments ────────────────────────────────────────
        /// <summary>
        /// Schedules a new appointment.
        /// Patients auto-fill their own patientId; Admin/Staff can specify any patient.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // If caller is a Patient, enforce they can only book for themselves
            var role           = User.FindFirstValue(ClaimTypes.Role);
            var patientIdClaim = User.FindFirstValue("patientId");

            if (role == "Patient")
            {
                if (!int.TryParse(patientIdClaim, out var callerPatientId))
                    return BadRequest(new { success = false, message = "Patient ID not found in token." });

                dto.PatientId = callerPatientId;   // override to prevent booking on behalf of others
            }

            try
            {
                _logger.LogInformation("Scheduling appointment: Patient {PatientId} → Doctor {DoctorId}",
                    dto.PatientId, dto.DoctorId);

                var created = await _appointmentService.CreateAppointmentAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id },
                    new { success = true, message = "Appointment scheduled successfully.", data = created });
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { success = false, message = ex.Message });
            }
        }

        // ─── PUT /api/appointments/{id} ────────────────────────────────────
        /// <summary>Updates an appointment's date, status, or notes. Admin/Staff only.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _appointmentService.UpdateAppointmentAsync(id, dto);
                if (updated == null)
                    return NotFound(new { success = false, message = $"Appointment with ID {id} not found." });

                return Ok(new { success = true, message = "Appointment updated successfully.", data = updated });
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { success = false, message = ex.Message });
            }
        }

        // ─── DELETE /api/appointments/{id} ─────────────────────────────────
        /// <summary>Removes an appointment record permanently. Admin/Staff only.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting appointment ID {Id}", id);
            var result = await _appointmentService.DeleteAppointmentAsync(id);

            if (!result)
                return NotFound(new { success = false, message = $"Appointment with ID {id} not found." });

            return Ok(new { success = true, message = $"Appointment {id} cancelled successfully." });
        }

        // ─── PATCH /api/appointments/{id}/cancel ───────────────────────────
        /// <summary>Patient cancels their own appointment.</summary>
        [HttpPatch("{id:int}/cancel")]
        [Authorize(Roles = "Patient")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelOwn(int id)
        {
            var patientIdClaim = User.FindFirstValue("patientId");
            if (!int.TryParse(patientIdClaim, out var patientId))
                return BadRequest(new { success = false, message = "Patient ID not found in token." });

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound(new { success = false, message = $"Appointment {id} not found." });

            if (appointment.PatientId != patientId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { success = false, message = "You can only cancel your own appointments." });

            var cancelDto = new UpdateAppointmentDto
            {
                AppointmentDate = appointment.AppointmentDate,
                Status          = "Cancelled"
            };

            try
            {
                var updated = await _appointmentService.UpdateAppointmentAsync(id, cancelDto);
                return Ok(new { success = true, message = "Appointment cancelled.", data = updated });
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { success = false, message = ex.Message });
            }
        }
    }
}
