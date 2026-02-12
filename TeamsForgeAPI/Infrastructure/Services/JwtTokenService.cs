using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TeamsForgeAPI.Infrastructure.Options;

namespace TeamsForgeAPI.Infrastructure.Services;

public class JwtTokenService
{
    private readonly JwtSettings? _settings;
    private readonly byte[] _key;

    public JwtTokenService(IOptions<JwtSettings> jwtOptions)
    {
        // Load JWT settings from configuration
        _settings = jwtOptions.Value;
        ArgumentNullException.ThrowIfNull(_settings);
        ArgumentNullException.ThrowIfNull(_settings.SigningKey);
        ArgumentNullException.ThrowIfNull(_settings.Audiences);
        ArgumentNullException.ThrowIfNull(_settings.Audiences[0]);
        ArgumentNullException.ThrowIfNull(_settings.Issuer);

        // Generate key from signing key
        _key = Encoding.ASCII.GetBytes(_settings.SigningKey);
    }

    // Provides a handler for creating and writing JWT tokens
    public static JwtSecurityTokenHandler TokenHandler => new();

    // Creates a JWT security token based on the provided claims
    public SecurityToken CreateSecurityToken(ClaimsIdentity identity)
    {
        var tokenDescriptor = GetTokenDescriptor(identity);
        return TokenHandler.CreateToken(tokenDescriptor);
    }

    // Converts the SecurityToken to its string representation (JWT)
    public string WriteToken(SecurityToken token)
    {
        return TokenHandler.WriteToken(token);
    }

    // Defines token properties such as claims, expiration, audience, and signing credentials
    public SecurityTokenDescriptor GetTokenDescriptor(ClaimsIdentity identity)
    {
        return new SecurityTokenDescriptor()
        {
            Subject = identity,
            Expires = DateTime.Now.AddDays(2), // Token expiration set to 2 days
            Audience = _settings.Audiences[0],
            Issuer = _settings.Issuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_key),
                SecurityAlgorithms.HmacSha256Signature // Secure algorithm for signing
            )
        };
    }
}
