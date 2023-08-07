using Discord;

namespace GrillBot.Common.Helpers;

public static class EmbedHelper
{
    public static List<List<EmbedFieldBuilder>> SplitToPages(IReadOnlyList<EmbedFieldBuilder> fields, EmbedBuilder embed)
    {
        var embedClone = embed.Build().ToEmbedBuilder();
        var pages = new List<List<EmbedFieldBuilder>>();

        for (var i = 0; i < fields.Count; i++)
        {
            var field = fields[i];
            embedClone.AddField(field);

            if (embedClone.Length > EmbedBuilder.MaxEmbedLength)
            {
                pages.Add(embedClone.Fields.Take(embedClone.Fields.Count - 1).ToList());
                embedClone.Fields.Clear();
                i--;
            }
            else if (embedClone.Fields.Count == EmbedBuilder.MaxFieldCount)
            {
                pages.Add(embedClone.Fields);
                embedClone.Fields.Clear();
            }
        }

        if (embedClone.Fields.Count > 0)
            pages.Add(embedClone.Fields);
        return pages;
    }
}
