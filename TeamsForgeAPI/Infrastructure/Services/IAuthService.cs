using TeamsForgeAPI.Domain.Dtos;

namespace TeamsForgeAPI.Infrastructure.Services;

public interface IAuthService
{
    Task<UserDto> RegisterAsync(RegisterDto dto);
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string userId);
    Task<MessageResponseDto> ForgotPasswordAsync(string email);
    Task<MessageResponseDto> ResetPasswordAsync(string token, string newPassword);
    Task<UserDto> GetProfileAsync(string userId);
    Task<UserDto> UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task<MessageResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task<MessageResponseDto> VerifyEmailAsync(string token);
}