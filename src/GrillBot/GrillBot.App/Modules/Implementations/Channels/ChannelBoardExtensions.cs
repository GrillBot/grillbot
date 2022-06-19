using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;

namespace GrillBot.App.Modules.Implementations.Channels;

public static class ChannelBoardExtensions
{
    public static EmbedBuilder WithChannelboard(this EmbedBuilder builder, IUser user, IGuild guild, IEnumerable<KeyValuePair<string, long>> data,
        Func<ulong, ITextChannel> channelFinder, int skip, int page = 0)
    {
        builder.WithFooter(user);
        builder.WithMetadata(new ChannelboardMetadata { Page = page, GuildId = guild.Id });

        builder.WithAuthor("Statistika aktivity v kanálech");
        builder.WithColor(Color.Blue);
        builder.WithCurrentTimestamp();

        builder.WithDescription(string.Join("\n", data.Select((o, i) =>
        {
            var channel = channelFinder(o.Key.ToUlong());
            return $"**{i + skip + 1,2}.** #{channel.Name} ({FormatHelper.FormatMessagesToCzech(o.Value)})";
        })));

        return builder;
    }
}
