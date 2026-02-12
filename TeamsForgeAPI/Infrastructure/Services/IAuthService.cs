using TeamsForgeAPI.Domain.Dtos;

namespace TeamsForgeAPI.Infrastructure.Services;

public interface IAuthService
{
    Task<AuthenticationResult> RegisterAsync(RegisterDto dto);
    Task<AuthenticationResult> LoginAsync(LoginDto dto);
}