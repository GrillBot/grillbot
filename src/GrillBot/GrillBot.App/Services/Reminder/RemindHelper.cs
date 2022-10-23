using Discord.Net;
using GrillBot.Common;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Services.Reminder;

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
            var msg = await destination.SendMessageAsync(embed: embed.Build());
            if (force) return msg.Id.ToString();

            var hours = Emojis.NumberToEmojiMap.Where(o => o.Key > 0).Select(o => o.Value);
            await msg.AddReactionsAsync(hours);
            return msg.Id.ToString();
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            return NotSentRemind;
        }
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
}
