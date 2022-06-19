using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Database.Entity;

namespace GrillBot.App.Modules.Implementations.Points;

public class PointsBoardBuilder : EmbedBuilder
{
    public PointsBoardBuilder WithBoard(IUser user, IGuild guild, IEnumerable<GuildUser> data, Func<ulong, IGuildUser> userFinder, int skip, int page = 0)
    {
        this.WithFooter(user);
        this.WithMetadata(new PointsBoardMetadata { GuildId = guild.Id, Page = page });

        WithAuthor("Statistika aktivity dle bodů");
        WithColor(Discord.Color.Blue);
        WithCurrentTimestamp();

        WithDescription(string.Join("\n", data.Select((o, i) =>
        {
            var foundUser = userFinder(o.UserId.ToUlong());
            return $"**{i + skip + 1,2}.** {(foundUser == null ? "*(Neznámý uživatel)*" : foundUser.GetDisplayName())} ({FormatHelper.FormatPointsToCzech(o.Points)})";
        })));

        return this;
    }
}
