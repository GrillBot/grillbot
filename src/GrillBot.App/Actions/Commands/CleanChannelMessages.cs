using Discord.Net;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Core.Helpers;

namespace GrillBot.App.Actions.Commands;

public class CleanChannelMessages : CommandAction
{
    private const long DiscordEpoch = 1420070400000L;

    private ITextsManager Texts { get; }
    private IMessageCacheManager MessageCache { get; }

    private RequestOptions RequestOptions => new()
    {
        Timeout = 30000,
        RetryMode = RetryMode.AlwaysRetry,
        AuditLogReason = $"GrillBot channel clean command. Exexcuted {Context.User.GetFullName()} in {Context.Channel.Name}"
    };

    public CleanChannelMessages(ITextsManager texts, IMessageCacheManager messageCache)
    {
        Texts = texts;
        MessageCache = messageCache;
    }

    public async Task<string> ProcessAsync(string criterium, IGuildChannel? channel)
    {
        if (channel is not null && channel is not ITextChannel)
            return Texts["ChannelModule/Clean/UnsupportedChannel", Locale];

        var guildTextChannel = channel as ITextChannel ?? (ITextChannel)Context.Channel;
        var countOrIdValue = ParseValue(criterium);
        var messages = await GetMessagesAsync(countOrIdValue, guildTextChannel);
        var count = countOrIdValue < DiscordEpoch ? Convert.ToInt32(countOrIdValue) : 0;
        var (totalCount, pinnedCount) = await ProcessMessagesAsync(messages, count);

        return Texts["ChannelModule/Clean/ResultMessage", Locale].FormatWith(totalCount, pinnedCount);
    }

    private static ulong ParseValue(string countOrMessage)
    {
        var messageLink = MessageHelper.DiscordMessageUriRegex().Match(countOrMessage);
        return messageLink.Success ? messageLink.Groups[3].Value.ToUlong() : countOrMessage.ToUlong();
    }

    private static async Task<IEnumerable<IMessage>> GetMessagesAsync(ulong countOrId, IMessageChannel channel)
    {
        if (countOrId < DiscordEpoch) // Value before discord epoch means count of messages.
            return await channel.GetMessagesAsync(Convert.ToInt32(countOrId) + 1).FlattenAsync();

        // Value after discord epoch means message ID.
        return await channel.GetMessagesAsync(countOrId, Direction.After, int.MaxValue).FlattenAsync();
    }


    private async Task<(int total, int pinned)> ProcessMessagesAsync(IEnumerable<IMessage> messages, int count)
    {
        var messagesQuery = messages.Where(o => o.Id != Context.Interaction.Id && o.Interaction?.Id != Context.Interaction.Id);
        if (count > 0) messagesQuery = messagesQuery.OrderByDescending(o => o.CreatedAt).Take(count);
        var messagesData = messagesQuery.ToList();

        var pinnedCount = messagesData.Count(o => o.IsPinned);
        foreach (var message in messagesData)
            await DeleteMessageAsync(message);

        return (messagesData.Count, pinnedCount);
    }

    private async Task DeleteMessageAsync(IMessage message)
    {
        try
        {
            await MessageCache.DeleteAsync(message.Id);
            await message.DeleteAsync(RequestOptions);
        }
        catch (HttpException ex) when (IsExpectedApiError(ex))
        {
            // Ignore expected exceptions.
        }
    }

    private static bool IsExpectedApiError(HttpException ex)
        => ex.IsExpectedOutageError() || ex.DiscordCode == DiscordErrorCode.UnknownChannel || ex.DiscordCode == DiscordErrorCode.UnknownMessage;
}
