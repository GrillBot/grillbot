namespace GrillBot.Data.Models.API
{
    public class MessageResponse
    {
        public string Message { get; set; }

        public MessageResponse() { }

        public MessageResponse(string message)
        {
            Message = message;
        }
    }
}
