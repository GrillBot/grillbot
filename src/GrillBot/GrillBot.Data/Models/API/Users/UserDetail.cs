using Discord;
using GrillBot.Data.Extensions.Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Data.Models.API.Users;

public class UserDetail
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }
    public string Note { get; set; }
    public long Flags { get; set; }
    public bool HaveBirthday { get; set; }
    public List<GuildUserDetail> Guilds { get; set; }
    public UserStatus Status { get; set; }
    public List<string> ActiveClients { get; set; }
    public bool IsKnown { get; set; }
    public string AvatarUrl { get; set; }
    public TimeSpan? SelfUnverifyMinimalTime { get; set; }
    public DateTime? RegisteredAt { get; set; }

    public UserDetail() { }

    public UserDetail(Database.Entity.User entity, IUser user, IDiscordClient discordClient)
    {
        Id = entity.Id;
        Username = entity.Username;
        Discriminator = entity.Discriminator;
        Note = entity.Note;
        Flags = entity.Flags;
        HaveBirthday = entity.Birthday != null;
        Guilds = entity.Guilds.Select(o => new GuildUserDetail(o, discordClient.GetGuildAsync(Convert.ToUInt64(o.GuildId)).Result)).OrderBy(o => o.Guild.Name).ToList();
        IsKnown = user != null;
        SelfUnverifyMinimalTime = entity.SelfUnverifyMinimalTime;

        if (IsKnown)
        {
            ActiveClients = user.ActiveClients.Select(o => o.ToString()).OrderBy(o => o).ToList();
            Status = user.Status;
            AvatarUrl = user.GetAvatarUri();
            RegisteredAt = user.CreatedAt.LocalDateTime;
        }
        else
        {
            AvatarUrl = CDN.GetDefaultUserAvatarUrl(0);
        }
    }

    public void RemoveSecretData()
    {
        Note = null;
        Guilds = Guilds.Where(o => o.IsUserInGuild).ToList();

        foreach (var guild in Guilds)
        {
            guild.Channels = guild.Channels.Where(o => !o.Channel.Name.StartsWith("Imported")).ToList();
        }
    }
}
