using Discord;

namespace GrillBot.Common.Helpers;

public static class ComponentsHelper
{
    public static MessageComponent? CreatePaginationComponents(int currentPage, int maxPages, string customIdPrefix)
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

    public static MessageComponent? CreateWrappedComponents(IReadOnlyList<IMessageComponent> components)
    {
        if (components.Count == 0)
            return null;
        if (components.Count > ComponentBuilder.MaxActionRowCount * ActionRowBuilder.MaxChildCount)
            throw new ArgumentException("Unable to create wrapped components for more than 5 rows.", nameof(components));

        var builder = new ComponentBuilder();
        var row = new ActionRowBuilder();

        for (var i = 0; i < components.Count; i++)
        {
            row.AddComponent(components[i]);

            if (row.Components.Count != ActionRowBuilder.MaxChildCount)
                continue;

            builder.AddRow(row);
            row = new ActionRowBuilder();
        }

        if (row.Components.Count > 0)
            builder.AddRow(row);

        return builder.Build();
    }
}
