using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Database.Enums;

namespace GrillBot.Data.Models.API.Users;

public class UserDetail
{
    public string Id { get; set; } = null!;
    public string Username { get; set; } = null!;
    public long Flags { get; set; }
    public bool HaveBirthday { get; set; }
    public List<GuildUserDetail> Guilds { get; set; } = new();
    public UserStatus Status { get; set; }
    public List<string> ActiveClients { get; set; } = new();
    public bool IsKnown { get; set; }
    public string AvatarUrl { get; set; } = null!;
    public TimeSpan? SelfUnverifyMinimalTime { get; set; }
    public DateTime? RegisteredAt { get; set; }
    public string? Language { get; set; }

    public void RemoveSecretData()
    {
        Guilds = Guilds.Where(o => o.IsUserInGuild).ToList();

        foreach (var guild in Guilds)
        {
            guild.Channels = guild.Channels
                .FindAll(o => !o.Channel.Name.StartsWith("Imported") && (o.Channel.Flags & (long)ChannelFlag.Deleted) == 0);
        }
    }
}
