using GrillBot.Common;

namespace GrillBot.App.Helpers;

public static class RemindHelper
{
    public static MessageComponent CreateCopyButton(long remindId)
        => new ComponentBuilder().WithButton(customId: $"remind_copy:{remindId}", emote: Emojis.PersonRisingHand).Build();
}
