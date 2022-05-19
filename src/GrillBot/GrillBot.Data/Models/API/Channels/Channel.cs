using Discord;
using System.Diagnostics;

namespace GrillBot.Data.Models.API.Channels;

[DebuggerDisplay("{Name} ({Id}, {Type})")]
public class Channel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public ChannelType? Type { get; set; }
    public long Flags { get; set; }
}
