using System;
using Microsoft.AspNetCore.Identity;

namespace TeamsForgeAPI.Domain.Entities;

public class ApplicationUser : IdentityUser
{

    //Custom fields 
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties 
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

}


