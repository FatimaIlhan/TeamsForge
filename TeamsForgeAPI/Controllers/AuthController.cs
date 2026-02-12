using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TeamsForgeAPI.Domain.Dtos;
using TeamsForgeAPI.Domain.Entities;

namespace TeamsForgeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
            private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
             
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized("Invalid credentials");

        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
           // new Claim(CustomClaimTypes.SystemRole, user.SystemRole.ToString())
        };

        // Add team roles (one claim per role)
        // foreach (var teamRole in user.TeamRoles)
        // {
        //     authClaims.Add(new Claim(CustomClaimTypes.TeamRole, teamRole.Role.ToString()));
        // }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
        );

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.UtcNow.AddHours(1),
            claims: authClaims,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        // Generate refresh token
        var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        // Store refresh token in database 
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);
        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            refreshToken = refreshToken,
            expiration = token.ValidTo
        });

    }



    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return BadRequest("User not found"); 
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // For production: send via email
        await SendEmailAsync(user.Email!, "Password Reset", $"Use this token to reset your password: {token}");
        // For Swagger/dev: return token in response
        return Ok(new { message = "Password reset token generated", token }); }

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpSettings = _configuration.GetSection("Smtp").Get<SmtpSettings>();

        using var client = new SmtpClient(smtpSettings.Host)
        {
            Port = smtpSettings.Port,
            EnableSsl = smtpSettings.EnableSsl,
            Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password)
        };

        var mailMessage = new MailMessage(smtpSettings.From, toEmail, subject, body);
        await client.SendMailAsync(mailMessage);
    }


    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model) 
    { 
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null) return BadRequest("User not found");
        var result = await _userManager.ResetPasswordAsync(user,
            model.Token,
            model.NewPassword); 
        if (!result.Succeeded) return BadRequest(result.Errors);
        return Ok(new { message = "Password reset successful" }); }
}
}
