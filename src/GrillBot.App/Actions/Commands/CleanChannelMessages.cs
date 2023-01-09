using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Actions.Commands;

public class CleanChannelMessages : CommandAction
{
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

    public async Task<string> ProcessAsync(int count, ITextChannel? channel)
    {
        channel ??= (ITextChannel)Context.Channel;

        var messages = await channel.GetMessagesAsync(count).FlattenAsync();
        var (totalCount, pinnedCount) = await ProcessMessagesAsync(channel, messages);

        return Texts["ChannelModule/Clean/ResultMessage", Locale].FormatWith(totalCount, pinnedCount);
    }

    private async Task<(int total, int pinned)> ProcessMessagesAsync(ITextChannel channel, IEnumerable<IMessage> messages)
    {
        var messagesData = messages.ToList();
        var older = messagesData.Where(o => o.Id != Context.Interaction.Id && IsOld(o)).ToList();
        var newer = messagesData.Where(o => o.Id != Context.Interaction.Id && !IsOld(o)).ToList();
        var pinnedCount = messagesData.Count(o => o.IsPinned);

        await channel.DeleteMessagesAsync(newer, RequestOptions);
        foreach (var message in older)
            await message.DeleteAsync(RequestOptions);

        return (older.Count + newer.Count, pinnedCount);
    }

    private static bool IsOld(ISnowflakeEntity message)
        => (DateTime.UtcNow - message.CreatedAt).TotalDays >= 14.0;
}
