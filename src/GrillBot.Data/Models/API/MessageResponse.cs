namespace GrillBot.Data.Models.API;

public class MessageResponse
{
    public string Message { get; set; } = null!;

    public MessageResponse() { }

    public MessageResponse(string message)
    {
        Message = message;
    }
}