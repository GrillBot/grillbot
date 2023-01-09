using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Actions.Commands;

public class PurgePins : CommandAction
{
    private ITextsManager Texts { get; }

    private RequestOptions RequestOptions => new()
    {
        Timeout = 30000,
        RetryMode = RetryMode.AlwaysRetry,
        AuditLogReason = $"GrillBot pins purge. Exexcuted {Context.User.GetFullName()} in {Context.Channel.Name}"
    };

    public PurgePins(ITextsManager texts)
    {
        Texts = texts;
    }

    public async Task<string> ProcessAsync(int count, ITextChannel? channel)
    {
        channel ??= (ITextChannel)Context.Channel;

        var messages = await channel.GetPinnedMessagesAsync();
        var pins = messages.OfType<IUserMessage>().ToList();
        if (pins.Count < count) count = pins.Count;

        foreach (var pin in pins.Take(count))
            await pin.UnpinAsync(RequestOptions);

        return Texts["Pins/UnpinCount", Locale].FormatWith(count);
    }
}
