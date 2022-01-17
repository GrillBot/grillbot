using Discord;
using Discord.WebSocket;
using GrillBot.Data.Extensions;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Infrastructure.Embeds;
using GrillBot.Database.Entity;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace GrillBot.Data.Modules.Implementations.Reminder;

public static class RemindListExtensions
{
    public static async Task<EmbedBuilder> WithRemindListAsync(this EmbedBuilder embed, List<RemindMessage> data, DiscordSocketClient client, IUser forUser, IUser user, int page)
    {
        embed.WithFooter(user);
        embed.WithMetadata(new RemindListMetadata() { OfUser = forUser.Id, Page = page });

        embed.WithAuthor($"Seznam čekajících upozornění pro {forUser.GetDisplayName()}.");
        embed.WithColor(Color.Blue);
        embed.WithCurrentTimestamp();

        if (data.Count == 0)
        {
            embed.WithDescription($"Pro uživatele {forUser.GetDisplayName()} nečekají žádné upozornění.");
        }
        else
        {
            foreach (var remind in data)
            {
                var from = await client.FindUserAsync(Convert.ToUInt64(remind.FromUserId));

                var title = $"#{remind.Id} - Od {from.GetDisplayName()} v {remind.At.ToCzechFormat()} (za {(remind.At - DateTime.Now).Humanize(culture: new CultureInfo("cs-CZ"))})";
                embed.AddField(title, remind.Message.Substring(0, Math.Min(remind.Message.Length, EmbedFieldBuilder.MaxFieldValueLength)));
            }
        }

        return embed;
    }
}
