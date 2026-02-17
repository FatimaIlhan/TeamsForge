namespace TeamsForgeAPI.Domain.Dtos;

public class MessageResponseDto
{
    public string Message { get; set; } = string.Empty;

    public MessageResponseDto(string message)
    {
        Message = message;
    }
}
