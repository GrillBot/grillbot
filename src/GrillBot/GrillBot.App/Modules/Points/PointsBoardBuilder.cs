using Discord;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure.Embeds;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.App.Modules.Points
{
    public class PointsBoardBuilder : EmbedBuilder
    {
        public PointsBoardBuilder WithBoard(IUser user, IGuild guild, List<KeyValuePair<string, long>> data,
            Func<ulong, IGuildUser> userFinder, int skip, int page = 0)
        {
            this.WithFooter(user);
            this.WithMetadata(new PointsBoardMetadata() { GuildId = guild.Id, PageNumber = page });

            WithAuthor("Statistika aktivity dle bodů");
            WithColor(Discord.Color.Blue);
            WithCurrentTimestamp();

            WithDescription(string.Join("\n", data.Select((o, i) =>
            {
                var user = userFinder(Convert.ToUInt64(o.Key));
                return $"**{i + skip + 1,2}.** {user.GetDisplayName()} ({FormatHelper.FormatPointsToCzech(o.Value)})";
            })));

            return this;
        }
    }
}
