using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Core.Helpers;

namespace GrillBot.App.Actions.Commands;

public class CleanChannelMessages : CommandAction
{
    private const long DiscordEpoch = 1420070400000L;

    private ITextsManager Texts { get; }

    private RequestOptions RequestOptions => new()
    {
        Timeout = 30000,
        RetryMode = RetryMode.AlwaysRetry,
        AuditLogReason = $"GrillBot channel clean command. Exexcuted {Context.User.GetFullName()} in {Context.Channel.Name}"
    };

    public CleanChannelMessages(ITextsManager texts)
    {
        Texts = texts;
    }

    public async Task<string> ProcessAsync(string criterium, ITextChannel? channel)
    {
        channel ??= (ITextChannel)Context.Channel;

        var countOrIdValue = ParseValue(criterium);
        var messages = await GetMessagesAsync(countOrIdValue, channel);
        var count = countOrIdValue < DiscordEpoch ? Convert.ToInt32(countOrIdValue) : 0;
        var (totalCount, pinnedCount) = await ProcessMessagesAsync(channel, messages, count);

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


    private async Task<(int total, int pinned)> ProcessMessagesAsync(ITextChannel channel, IEnumerable<IMessage> messages, int count)
    {
        var messagesQuery = messages.Where(o => o.Id != Context.Interaction.Id && o.Interaction?.Id != Context.Interaction.Id);
        if (count > 0) messagesQuery = messagesQuery.OrderByDescending(o => o.CreatedAt).Take(count);
        var messagesData = messagesQuery.ToList();

        var pinnedCount = messagesData.Count(o => o.IsPinned);
        var deleteIndividually = messagesData.Where(o => IsOld(o) || (o.Source == MessageSource.System && o.Type != MessageType.ApplicationCommand && o.Type != MessageType.ContextMenuCommand))
            .ToDictionary(o => o.Id, o => o);
        var deletePerGroups = messagesData.Where(o => !deleteIndividually.ContainsKey(o.Id)).ToList();

        // Only one message cannot be deleted in batch.
        if (deletePerGroups.Count == 1)
        {
            deleteIndividually.Add(deletePerGroups[0].Id, deletePerGroups[0]);
            deletePerGroups.Clear();
        }

        for (var i = 0; i < deletePerGroups.Count; i += Math.Min(deletePerGroups.Count, 100))
        {
            var group = deletePerGroups.Skip(i).Take(Math.Min(deletePerGroups.Count, 100)).ToList(); // Can delete max 100 messages per request.
            await channel.DeleteMessagesAsync(group, RequestOptions);
        }

        foreach (var message in deleteIndividually)
            await message.Value.DeleteAsync(RequestOptions);

        return (deleteIndividually.Count + deletePerGroups.Count, pinnedCount);
    }

    private static bool IsOld(ISnowflakeEntity message)
        => (DateTime.UtcNow - message.CreatedAt).TotalDays >= 14.0;
}
