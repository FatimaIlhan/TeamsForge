using System;

namespace TeamsForgeAPI.Domain.Entities;

public class UserRole
{
    public string? UserId { get; set; }
    public int RoleId { get; set; }
 
    public ApplicationUser User { get; set; } = null!;
  

}