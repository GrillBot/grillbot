using Discord;
using Discord.WebSocket;
using System;

namespace GrillBot.Data.Models.Invite
{
    public class InviteMetadata
    {
        public ulong GuildId { get; internal set; }
        public string Code { get; internal set; }
        public int Uses { get; internal set; }
        public bool IsVanity { get; internal set; }
        public ulong? CreatorId { get; internal set; }
        public DateTime? CreatedAt { get; internal set; }

        internal InviteMetadata(string code, int? uses, IGuild guild)
        {
            Code = code;
            Uses = uses ?? 0;
            GuildId = guild.Id;
        }

        static public InviteMetadata FromEntity(Database.Entity.Invite invite, SocketGuild guild)
        {
            return new InviteMetadata(invite.Code, invite.UsedUsers.Count, guild)
            {
                IsVanity = guild.VanityURLCode == invite.Code,
                CreatorId = Convert.ToUInt64(invite.CreatorId),
                CreatedAt = invite.CreatedAt
            };
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
}
