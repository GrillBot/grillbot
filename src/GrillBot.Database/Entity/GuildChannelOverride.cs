namespace GrillBot.Database.Entity;

public class GuildChannelOverride
{
    public ulong ChannelId { get; set; }
    public ulong AllowValue { get; set; }
    public ulong DenyValue { get; set; }
}
