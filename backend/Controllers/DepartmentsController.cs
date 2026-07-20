using HospitalApp.Data;
using HospitalApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalApp.Controllers
{
    /// <summary>
    /// REST API Controller for Department management.
    ///
    /// Endpoints:
    ///   GET  /api/departments        → List all departments
    ///   GET  /api/departments/{id}   → Get one department
    ///   POST /api/departments        → Create department
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly HospitalDbContext _context;
        private readonly ILogger<DepartmentsController> _logger;

        public DepartmentsController(HospitalDbContext context, ILogger<DepartmentsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>Returns all departments sorted by name.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var depts = await (
                from d in _context.Departments
                orderby d.Name
                select new { d.Id, d.Name, d.Code, DoctorCount = d.Doctors.Count }
            ).ToListAsync();

            return Ok(new { success = true, count = depts.Count, data = depts });
        }

        /// <summary>Returns a single department by ID.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var dept = await _context.Departments
                .Include(d => d.Doctors)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dept == null)
                return NotFound(new { success = false, message = $"Department {id} not found." });

            return Ok(new { success = true, data = new { dept.Id, dept.Name, dept.Code } });
        }

        /// <summary>Creates a new department. Admin only.</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Departments.AnyAsync(d => d.Code == req.Code.ToUpper()))
                return Conflict(new { success = false, message = $"Department code '{req.Code}' already exists." });

            var dept = new Department { Name = req.Name.Trim(), Code = req.Code.ToUpper().Trim() };
            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Department created: {Name} ({Code})", dept.Name, dept.Code);
            return CreatedAtAction(nameof(GetById), new { id = dept.Id },
                new { success = true, data = new { dept.Id, dept.Name, dept.Code } });
        }
    }

    public record CreateDepartmentRequest(
        [property: System.ComponentModel.DataAnnotations.Required]
        [property: System.ComponentModel.DataAnnotations.StringLength(100, MinimumLength = 2)]
        string Name,
        [property: System.ComponentModel.DataAnnotations.Required]
        [property: System.ComponentModel.DataAnnotations.StringLength(10, MinimumLength = 2)]
        string Code
    );
}
