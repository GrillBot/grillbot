using Discord;

namespace GrillBot.Data.Models.API.Channels;

public class Channel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public ChannelType? Type { get; set; }
}
