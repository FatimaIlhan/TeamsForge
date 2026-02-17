using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamsForgeAPI.Domain.Dtos;
using TeamsForgeAPI.Infrastructure.Services;

namespace TeamsForgeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="dto">Registration details</param>
    /// <returns>UserDto with registered user info</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto)
    {
        try
        {
            _logger.LogInformation("Register attempt for email: {Email}", dto.Email);
            var user = await _authService.RegisterAsync(dto);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Registration error: {Message}", ex.Message);
            return StatusCode(500, new { message = "An error occurred during registration." });
        }
    }

    /// <summary>
    /// User login
    /// </summary>
    /// <param name="dto">Login credentials</param>
    /// <returns>LoginResponseDto with tokens and user info</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Login failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Login error: {Message}", ex.Message);
            return StatusCode(500, new { message = "An error occurred during login." });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="dto">Refresh token</param>
    /// <returns>LoginResponseDto with new tokens</returns>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        try
        {
            _logger.LogInformation("Refresh token attempt");
            var result = await _authService.RefreshTokenAsync(dto.RefreshToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Token refresh error: {Message}", ex.Message);
            return StatusCode(500, new { message = "An error occurred during token refresh." });
        }
    }

    /// <summary>
    /// Logout user and invalidate refresh token
    /// </summary>
    /// <param name="dto">Refresh token</param>
    /// <returns>204 No Content</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found in token." });

            _logger.LogInformation("Logout for user: {UserId}", userId);
            await _authService.LogoutAsync(userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError("Logout error: {Message}", ex.Message);
            return StatusCode(500, new { message = "An error occurred during logout." });
        }
    }

    /// <summary>
    /// Request password reset link
    /// </summary>
    /// <param name="dto">User email</param>
    /// <returns>Message response</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            _logger.LogInformation("Forgot password request for email: {Email}", dto.Email);
            var result = await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Forgot password failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Forgot password error: {Message}", ex.Message);
            return StatusCode(500, new { message = "An error occurred while processing your request." });
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    /// <param name="dto">Reset token and new password</param>
    /// <returns>Message response</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponseDto>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            _logger.LogInformation("Reset password attempt");
            var result = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Reset password failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { message = "Password reset functionality is not yet implemented." });
        }
        catch (Exception ex)
        {
            _logger.LogError("Reset password error: {Message}", ex.Message);
            return StatusCode(500, new { message = "An error occurred during password reset." });
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>UserDto with current user info</returns>
    [HttpGet("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found in token." });

            _logger.LogInformation("Get profile for user: {UserId}", userId);
            var user = await _authService.GetProfileAsync(userId);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Get profile failed: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Get profile error: {Message}", ex.Message);
            return StatusCode(500, new { message = "An error occurred while retrieving your profile." });
        }
    }

    /// <summary>
    /// Update user profile
    /// </summary>
    /// <param name="dto">Updated profile info</param>
    /// <returns>UserDto with updated user info</returns>
    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found in token." });

            _logger.LogInformation("Update profile for user: {UserId}", userId);
            var user = await _authService.UpdateProfileAsync(userId, dto);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Update profile failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Update profile error: {Message}", ex.Message);
            return StatusCode(500, new { message = "An error occurred while updating your profile." });
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="dto">Current and new password</param>
    /// <returns>Message response</returns>
    [HttpPut("change-password")]
    [Authorize]
    public async Task<ActionResult<MessageResponseDto>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not found in token." });

            _logger.LogInformation("Change password for user: {UserId}", userId);
            var result = await _authService.ChangePasswordAsync(userId, dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Change password failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError("Change password error: {Message}", ex.Message);
            return StatusCode(500, new { message = "An error occurred while changing your password." });
        }
    }

    /// <summary>
    /// Verify user email with token
    /// </summary>
    /// <param name="dto">Email verification token</param>
    /// <returns>Message response</returns>
    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<ActionResult<MessageResponseDto>> VerifyEmail([FromBody] EmailVerificationDto dto)
    {
        try
        {
            _logger.LogInformation("Verify email attempt");
            var result = await _authService.VerifyEmailAsync(dto.Token);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Email verification failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (NotImplementedException)
        {
            return StatusCode(501, new { message = "Email verification functionality is not yet implemented." });
        }
        catch (Exception ex)
        {
            _logger.LogError("Email verification error: {Message}", ex.Message);
            return StatusCode(500, new { message = "An error occurred during email verification." });
        }
    }
}
