using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Database.Models.Points;

namespace GrillBot.App.Modules.Implementations.Points;

public class PointsBoardBuilder : EmbedBuilder
{
    public PointsBoardBuilder WithBoard(IUser user, IGuild guild, IEnumerable<PointBoardItem> data, int skip, int page = 0)
    {
        this.WithFooter(user);
        this.WithMetadata(new PointsBoardMetadata { GuildId = guild.Id, Page = page });

        WithAuthor("Statistika aktivity dle bodů");
        WithColor(Discord.Color.Blue);
        WithCurrentTimestamp();

        WithDescription(
            string.Join(
                "\n",
                data.Select((o, i) => $"**{i + skip + 1,2}.** {o.GuildUser.FullName()} ({FormatHelper.FormatPointsToCzech(o.Points)})")
            )
        );

        return this;
    }
}
