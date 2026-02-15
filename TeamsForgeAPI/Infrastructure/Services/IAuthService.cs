using TeamsForgeAPI.Domain.Dtos;

namespace TeamsForgeAPI.Infrastructure.Services;

public interface IAuthService
{
    Task<AuthenticationResultDto> RegisterAsync(RegisterDto dto);
    Task<AuthenticationResultDto> LoginAsync(LoginDto dto);
}