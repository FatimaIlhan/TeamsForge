using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TeamsForgeAPI.Domain.Dtos;
using TeamsForgeAPI.Domain.Entities;
using TeamsForgeAPI.Infrastructure.Data;

namespace TeamsForgeAPI.Infrastructure.Services;

public class AuthService: IAuthService
{

    private readonly AppDbContext _ctx;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(
        AppDbContext ctx,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SignInManager<ApplicationUser> signInManager,
        JwtTokenService jwtTokenService)
    {
        _ctx = ctx;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthenticationResultDto> RegisterAsync(RegisterDto dto)
    {
        var identity = new ApplicationUser
         { 
         Email = dto.Email,
          UserName = dto.Email 
          
          };

        var createdIdentity = await _userManager.CreateAsync(identity, dto.Password);

        if (!createdIdentity.Succeeded)
        {
            throw new Exception(string.Join(", ", createdIdentity.Errors.Select(e => e.Description)));
        }

        var newClaims = new List<Claim>
        {
            new Claim("FirstName", dto.FirstName),
            new Claim("LastName", dto.LastName)
        };
        await _userManager.AddClaimsAsync(identity, newClaims);

       
        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, identity.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, identity.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, identity.Id.ToString())
        });
        claimsIdentity.AddClaims(newClaims);

        var token = _jwtTokenService.CreateSecurityToken(claimsIdentity);
        var jwtToken = _jwtTokenService.WriteToken(token);

        return new AuthenticationResultDto(jwtToken, identity.Id.ToString());
    }

    public async Task<AuthenticationResultDto> LoginAsync(LoginDto dto)
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

        return new AuthenticationResultDto(jwtToken, user.Id.ToString());
    }

}
