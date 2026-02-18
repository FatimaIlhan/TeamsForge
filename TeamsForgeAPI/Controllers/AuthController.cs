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

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = await _authService.RegisterAsync(dto);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponseDto(ex.Message));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
    {
        try
        {
            var response = await _authService.LoginAsync(dto);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponseDto(ex.Message));
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(dto.RefreshToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponseDto(ex.Message));
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<MessageResponseDto>> Logout()
    {
        try
        {
            var userId = GetUserId();
            await _authService.LogoutAsync(userId);
            return Ok(new MessageResponseDto("Logged out successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponseDto(ex.Message));
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<MessageResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            var result = await _authService.ForgotPasswordAsync(dto.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponseDto(ex.Message));
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<MessageResponseDto>> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            var result = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            return Ok(result);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new MessageResponseDto(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponseDto(ex.Message));
        }
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        try
        {
            var userId = GetUserId();
            var user = await _authService.GetProfileAsync(userId);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponseDto(ex.Message));
        }
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        try
        {
            var userId = GetUserId();
            var user = await _authService.UpdateProfileAsync(userId, dto);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponseDto(ex.Message));
        }
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<MessageResponseDto>> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        try
        {
            var userId = GetUserId();
            var result = await _authService.ChangePasswordAsync(userId, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponseDto(ex.Message));
        }
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<MessageResponseDto>> VerifyEmail([FromBody] EmailVerificationDto dto)
    {
        try
        {
            var result = await _authService.VerifyEmailAsync(dto.Token);
            return Ok(result);
        }
        catch (NotImplementedException ex)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new MessageResponseDto(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(new MessageResponseDto(ex.Message));
        }
    }

    private string GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("User ID claim is missing.");
        }

        return userId;
    }
}
