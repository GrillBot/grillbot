namespace GrillBot.App.Helpers;

public static class ComponentsHelper
{
    public static MessageComponent CreatePaginationComponents(int currentPage, int maxPages, string customIdPrefix)
    {
        if (maxPages < 2) return null;

        var builder = new ComponentBuilder();

        if (maxPages > 2)
            builder = builder.WithButton(customId: $"{customIdPrefix}:-1", emote: Emojis.MoveToFirst);

        builder = builder
            .WithButton(customId: $"{customIdPrefix}:{Math.Max(currentPage - 1, 0)}", emote: Emojis.MoveToPrev)
            .WithButton(customId: $"{customIdPrefix}:{currentPage + 1}", emote: Emojis.MoveToNext);

        if (maxPages > 2)
            builder = builder.WithButton(customId: $"{customIdPrefix}:{int.MaxValue}", emote: Emojis.MoveToLast);

        return builder.Build();
    }
}
