using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Common.Extensions.Discord;

namespace GrillBot.App.Modules.Implementations.User;

public static class UserAccessListExtensions
{
    public static EmbedBuilder WithUserAccessList(this EmbedBuilder embed, List<Tuple<string, List<string>>> data, IUser forUser, IUser user, IGuild guild, int page)
    {
        embed.WithFooter(user);
        embed.WithAuthor($"Seznam přístupů pro uživatele {forUser.GetFullName()}", forUser.GetUserAvatarUrl());
        embed.WithMetadata(new UserAccessListMetadata { ForUserId = forUser.Id, GuildId = guild.Id, Page = page });

        embed.WithColor(Color.Blue);
        embed.WithCurrentTimestamp();

        data.ForEach(o => embed.AddField(o.Item1, string.Join(" ", o.Item2)));
        return embed;
    }
}
