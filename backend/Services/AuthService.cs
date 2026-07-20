using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using HospitalApp.Data;
using HospitalApp.DTOs;
using HospitalApp.Models;
using HospitalApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HospitalApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly HospitalDbContext _db;
        private readonly IConfiguration _config;

        public AuthService(HospitalDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // ── Register ─────────────────────────────────────────────────────────
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Check duplicate email
            if (await _db.AppUsers.AnyAsync(u => u.Email == dto.Email.ToLower()))
                throw new InvalidOperationException("An account with this email already exists.");

            // Check duplicate username
            if (await _db.AppUsers.AnyAsync(u => u.Username == dto.Username))
                throw new InvalidOperationException("Username is already taken.");

            // Force role to "Patient" on public registration (only Admin can create Staff/Admin)
            var assignedRole = "Patient";

            int? patientId = null;

            // ── If registering as Patient, create a linked Patient record ──────
            if (assignedRole == "Patient")
            {
                if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    throw new InvalidOperationException("Phone number is required for patient registration.");
                if (!dto.Age.HasValue || dto.Age <= 0)
                    throw new InvalidOperationException("Age is required for patient registration.");
                if (string.IsNullOrWhiteSpace(dto.Gender))
                    throw new InvalidOperationException("Gender is required for patient registration.");

                var patient = new Patient
                {
                    FullName    = dto.Username,
                    Age         = dto.Age.Value,
                    Gender      = dto.Gender,
                    Disease     = "Not specified",
                    PhoneNumber = dto.PhoneNumber,
                    AdmissionDate = DateTime.UtcNow,
                    IsActive    = true
                };

                _db.Patients.Add(patient);
                await _db.SaveChangesAsync();
                patientId = patient.Id;
            }

            var user = new AppUser
            {
                Username     = dto.Username,
                Email        = dto.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role         = assignedRole,
                PatientId    = patientId,
                CreatedAt    = DateTime.UtcNow
            };

            _db.AppUsers.Add(user);
            await _db.SaveChangesAsync();

            return BuildToken(user);
        }

        // ── Login ─────────────────────────────────────────────────────────────
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _db.AppUsers
                .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            return BuildToken(user);
        }

        // ── JWT Factory ───────────────────────────────────────────────────────
        private AuthResponseDto BuildToken(AppUser user)
        {
            var jwtConfig  = _config.GetSection("Jwt");
            var key        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!));
            var creds      = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt  = DateTime.UtcNow.AddHours(double.Parse(jwtConfig["ExpiresInHours"]!));

            var claimsList = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,        user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email,      user.Email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role,                    user.Role),
                new Claim(JwtRegisteredClaimNames.Jti,        Guid.NewGuid().ToString())
            };

            // Include patientId claim for Patient-role users
            if (user.PatientId.HasValue)
                claimsList.Add(new Claim("patientId", user.PatientId.Value.ToString()));

            var token = new JwtSecurityToken(
                issuer:   jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims:   claimsList,
                expires:  expiresAt,
                signingCredentials: creds
            );

            return new AuthResponseDto
            {
                Token     = new JwtSecurityTokenHandler().WriteToken(token),
                Username  = user.Username,
                Email     = user.Email,
                Role      = user.Role,
                ExpiresAt = expiresAt,
                PatientId = user.PatientId
            };
        }
    }
}
