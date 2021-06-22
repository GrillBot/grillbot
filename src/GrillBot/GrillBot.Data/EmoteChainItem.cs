namespace GrillBot.Data
{
    public class EmoteChainItem
    {
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public string Emotes { get; set; }
        public int Count { get; set; }
    }
}
