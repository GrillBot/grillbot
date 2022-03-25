using Discord;
using System;

namespace GrillBot.Data.Models.Invite;

public class InviteMetadata
{
    public ulong GuildId { get; set; }
    public string Code { get; set; }
    public int Uses { get; set; }
    public bool IsVanity { get; set; }
    public ulong? CreatorId { get; set; }
    public DateTime? CreatedAt { get; set; }

    public InviteMetadata(string code, int? uses, IGuild guild)
    {
        Code = code;
        Uses = uses ?? 0;
        GuildId = guild.Id;
    }

    static public InviteMetadata FromDiscord(IInviteMetadata metadata)
    {
        return new InviteMetadata(metadata.Code, metadata.Uses, metadata.Guild)
        {
            CreatorId = metadata.Inviter?.Id,
            IsVanity = metadata.Guild.VanityURLCode == metadata.Code,
            CreatedAt = metadata.CreatedAt?.LocalDateTime
        };
    }

    public Database.Entity.Invite ToEntity()
    {
        return new Database.Entity.Invite()
        {
            CreatedAt = CreatedAt,
            Code = Code,
            CreatorId = CreatorId?.ToString(),
            GuildId = GuildId.ToString()
        };
    }
}
