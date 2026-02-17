namespace TeamsForgeAPI.Infrastructure.Services;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string token);
    Task SendPasswordResetEmailAsync(string email, string token);
    Task SendEmailAsync(string to, string subject, string htmlContent);
}
