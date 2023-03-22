﻿using Discord.Net;
using GrillBot.Common;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;

namespace GrillBot.App.Helpers;

public class RemindHelper
{
    public const string NotSentRemind = "0";

    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }

    public RemindHelper(IDiscordClient discordClient, ITextsManager texts)
    {
        DiscordClient = discordClient;
        Texts = texts;
    }

    public async Task<string> ProcessRemindAsync(Database.Entity.RemindMessage remind, bool force)
    {
        var embed = await CreateRemindEmbedAsync(remind, force);

        var destination = await DiscordClient.FindUserAsync(remind.ToUserId.ToUlong());
        if (destination == null) return NotSentRemind;

        try
        {
            var postponeComponents = !force ? CreatePostponeComponents() : null;
            var msg = await destination.SendMessageAsync(embed: embed.Build(), components: postponeComponents);
            return msg.Id.ToString();
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            return NotSentRemind;
        }
    }

    private static MessageComponent CreatePostponeComponents()
    {
        var builder = new ComponentBuilder();
        var row = new ActionRowBuilder();
        var emojisMap = Emojis.NumberToEmojiMap.Values.ToList();

        for (var i = 1; i < emojisMap.Count; i++)
        {
            var item = emojisMap[i];
            row.AddComponent(new ButtonBuilder(customId: $"remind_postpone:{i}", emote: item).Build());
            if (i % ActionRowBuilder.MaxChildCount != 0 && i != emojisMap.Count - 1) continue;

            builder.AddRow(row);
            row = new ActionRowBuilder();
        }

        return builder.Build();
    }

    private async Task<EmbedBuilder> CreateRemindEmbedAsync(Database.Entity.RemindMessage remind, bool force = false)
    {
        const string localeBase = "RemindModule/NotifyMessage/";

        var locale = TextsManager.FixLocale(remind.Language);
        var embed = new EmbedBuilder()
            .WithAuthor(DiscordClient.CurrentUser)
            .WithColor(force ? Color.Gold : Color.Green)
            .WithCurrentTimestamp()
            .WithTitle(Texts[force ? localeBase + "ForceTitle" : localeBase + "Title", locale])
            .AddField(Texts[localeBase + "Fields/Id", locale], remind.Id, true);

        if (remind.FromUserId != remind.ToUserId)
        {
            var fromUser = await DiscordClient.FindUserAsync(remind.FromUserId.ToUlong());

            if (fromUser != null)
                embed.AddField(Texts[localeBase + "Fields/From", locale], fromUser.GetFullName(), true);
        }

        if (remind.Postpone > 0)
            embed.AddField(Texts[localeBase + "Fields/Attention", locale], Texts[localeBase + "Postponed", locale].FormatWith(remind.Postpone));

        embed
            .AddField(Texts[localeBase + "Fields/Message", locale], remind.Message);

        if (!force)
            embed.AddField(Texts[localeBase + "Fields/Options", locale], Texts[localeBase + "Options", locale]);

        return embed;
    }

    public static MessageComponent CreateCopyButton(long remindId)
        => new ComponentBuilder().WithButton(customId: $"remind_copy:{remindId}", emote: Emojis.PersonRisingHand).Build();
}
