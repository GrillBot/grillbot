using Discord;

namespace GrillBot.Common.Extensions.Discord;

public static class EmbedExtensions
{
    public static EmbedBuilder WithFooter(this EmbedBuilder builder, IUser user)
    {
        return builder.WithFooter(user.GetDisplayName(), user.GetUserAvatarUrl());
    }

    public static EmbedFooterBuilder WithUser(this EmbedFooterBuilder builder, IUser user)
        => builder.WithText(user.GetDisplayName()).WithIconUrl(user.GetUserAvatarUrl());
}
