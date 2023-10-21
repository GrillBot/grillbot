using System;

namespace GrillBot.Data.Models.API.Channels;

public class UserGuildChannel
{
    public Channel Channel { get; set; } = null!;
    public long Count { get; set; }
    public DateTime FirstMessageAt { get; set; }
    public DateTime LastMessageAt { get; set; }
}
