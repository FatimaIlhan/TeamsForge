using System;

namespace TeamsForgeAPI.Domain.Dtos;

public class AuthenticationResultDto
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

    public AuthenticationResultDto(string token, string userId)
    {
        Token = token;
        UserId = userId;
    }
}
