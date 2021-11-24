using Discord;

namespace GrillBot.Data.Models.MessageCache
{
    public class CachedMessage
    {
        public IMessage Message { get; set; }
        public MessageMetadata Metadata { get; set; }

        public bool IsDeleted => Metadata.State == Enums.CachedMessageState.ToBeDeleted;

        public CachedMessage(IMessage message)
        {
            Message = message;
            Metadata = new MessageMetadata();
        }
    }
}
