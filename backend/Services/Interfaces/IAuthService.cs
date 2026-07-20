using HospitalApp.DTOs;

namespace HospitalApp.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>Registers a new user. Returns a JWT response or throws on failure.</summary>
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

        /// <summary>Authenticates an existing user. Returns a JWT response or throws on failure.</summary>
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
    }
}
