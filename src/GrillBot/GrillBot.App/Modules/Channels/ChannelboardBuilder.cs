﻿using Discord;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure.Embeds;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.App.Modules.Channels
{
    public class ChannelboardBuilder : EmbedBuilder
    {
        public ChannelboardBuilder WithChannelboard(IUser user, IGuild guild, List<KeyValuePair<string, long>> data,
            Func<ulong, ITextChannel> channelFinder, int skip, int page = 0)
        {
            this.WithFooter(user);
            this.WithMetadata(new ChannelboardMetadata() { PageNumber = page, GuildId = guild.Id });

            WithAuthor("Statistika aktivity v kanálech");
            WithColor(Discord.Color.Blue);
            WithCurrentTimestamp();

            WithDescription(string.Join("\n", data.Select((o, i) =>
            {
                var channel = channelFinder(Convert.ToUInt64(o.Key));
                return $"**{i + skip + 1,2}.** #{channel.Name} ({FormatHelper.FormatMessagesToCzech(o.Value)})";
            })));

            return this;
        }
    }
}