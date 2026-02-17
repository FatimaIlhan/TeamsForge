using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using TeamsForgeAPI.Domain.Dtos;
using TeamsForgeAPI.Domain.Entities;
using TeamsForgeAPI.Infrastructure.Data;

namespace TeamsForgeAPI.Infrastructure.Services;

public class AuthService : IAuthService
{

    private readonly AppDbContext _ctx;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IEmailService? _emailService;

    public AuthService(
        AppDbContext ctx,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwtTokenService,
        IEmailService? emailService = null)
    {
        _ctx = ctx;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
    }

    public async Task<UserDto> RegisterAsync(RegisterDto dto)
    {
        var userExists = await _userManager.FindByEmailAsync(dto.Email);
        if (userExists != null)
            throw new InvalidOperationException("User with this email already exists.");

        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DisplayName = dto.DisplayName,
            EmailConfirmed = false,
            TenantId = Guid.NewGuid()
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Generate email verification token
        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // TODO: Send verification email to user
        // await _emailService?.SendEmailVerificationAsync(user.Email, emailToken);

        return MapToUserDto(user);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new InvalidOperationException("Invalid login attempt.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            throw new InvalidOperationException("Invalid login credentials.");

        var claims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        });
        claimsIdentity.AddClaims(claims);
        foreach (var role in roles)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        var token = _jwtTokenService.CreateSecurityToken(claimsIdentity);
        var jwtToken = _jwtTokenService.WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return new LoginResponseDto
        {
            Token = jwtToken,
            RefreshToken = refreshToken,
            User = MapToUserDto(user)
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = _ctx.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);
        if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired refresh token.");

        var claims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);

        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        });
        claimsIdentity.AddClaims(claims);
        foreach (var role in roles)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        var token = _jwtTokenService.CreateSecurityToken(claimsIdentity);
        var jwtToken = _jwtTokenService.WriteToken(token);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return new LoginResponseDto
        {
            Token = jwtToken,
            RefreshToken = newRefreshToken,
            User = MapToUserDto(user)
        };
    }

    public async Task LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);
        }
    }

    public async Task<MessageResponseDto> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new InvalidOperationException("User with this email not found.");

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        // TODO: Send password reset email to user
        // await _emailService?.SendPasswordResetEmailAsync(user.Email, resetToken);

        return new MessageResponseDto("Password reset link has been sent to your email.");
    }

    public async Task<MessageResponseDto> ResetPasswordAsync(string token, string newPassword)
    {
        // For now, we'll need to find the user from token context
        // In a real app, you'd decode the token to find the user
        throw new NotImplementedException("Reset password token handling needs proper implementation.");
    }

    public async Task<UserDto> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        return MapToUserDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.DisplayName = dto.DisplayName;
        user.AvatarUrl = dto.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException("Failed to update profile.");

        return MapToUserDto(user);
    }

    public async Task<MessageResponseDto> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        return new MessageResponseDto("Password changed successfully.");
    }

    public async Task<MessageResponseDto> VerifyEmailAsync(string token)
    {
        // For now, we'll need to find the user from token context
        throw new NotImplementedException("Email verification token handling needs proper implementation.");
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    private UserDto MapToUserDto(ApplicationUser user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            TimeZone = user.TimeZone,
            Language = user.Language,
            EmailConfirmed = user.EmailConfirmed
        };
    }
}

