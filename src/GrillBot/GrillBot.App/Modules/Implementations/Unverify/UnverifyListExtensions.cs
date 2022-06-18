using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Data.Extensions;
using GrillBot.Data.Models.Unverify;

namespace GrillBot.App.Modules.Implementations.Unverify;

public static class UnverifyListExtensions
{
    public static EmbedBuilder WithUnverifyList(this EmbedBuilder embed, UnverifyUserProfile profile, SocketGuild guild, IUser forUser, int page)
    {
        embed.WithFooter(forUser);

        embed.WithAuthor(profile.Destination.GetFullName(), profile.Destination.GetUserAvatarUrl());
        embed.WithMetadata(new UnverifyListMetadata() { Page = page, GuildId = guild.Id });

        var color = new[] { profile.RolesToKeep, profile.RolesToRemove }.SelectMany(o => o)
            .Where(o => o.Color != Color.Default)
            .OrderByDescending(o => o.Position)
            .Select(o => o.Color)
            .FirstOrDefault();

        embed
            .WithColor(color)
            .WithCurrentTimestamp()
            .WithTitle("Dočasné odebrání přístupu")
            .AddField("Začátek", profile.Start.ToCzechFormat(), true)
            .AddField("Konec", profile.End.ToCzechFormat(), true)
            .AddField("Konec za", (profile.End - DateTime.Now).Humanize(culture: new CultureInfo("cs-CZ")), true)
            .AddField("Selfunverify", FormatHelper.FormatBooleanToCzech(profile.IsSelfUnverify), true);

        if (!string.IsNullOrEmpty(profile.Reason))
            embed.AddField("Důvod", profile.Reason, false);

        if (profile.RolesToKeep.Count > 0)
        {
            foreach (var chunk in profile.RolesToKeep.Select(o => o.Mention).SplitInParts(30))
            {
                embed.AddField("Ponechané role", string.Join(" ", chunk));
            }
        }

        if (profile.RolesToRemove.Count > 0)
        {
            foreach (var chunk in profile.RolesToRemove.Select(o => o.Mention).SplitInParts(30))
            {
                embed.AddField("Odebrané role", string.Join(" ", chunk));
            }
        }

        if (profile.ChannelsToKeep.Count > 0)
        {
            var groups = profile.ChannelsToKeep.Select(o => guild.GetChannel(o.ChannelId))
                .Where(o => o != null)
                .Select(o => o.GetMention())
                .SplitInParts(30);

            foreach (var group in groups)
            {
                embed.AddField("Ponechané kanály", string.Join(" ", group));
            }
        }

        if (profile.ChannelsToRemove.Count > 0)
        {
            var groups = profile.ChannelsToRemove.Select(o => guild.GetChannel(o.ChannelId))
                .Where(o => o != null)
                .Select(o => o.GetMention())
                .SplitInParts(30);

            foreach (var group in groups)
            {
                embed.AddField("Odebrané kanály", string.Join(" ", group));
            }
        }

        return embed;
    }
}
