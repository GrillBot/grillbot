using GrillBot.App.Infrastructure.Embeds;

namespace GrillBot.App.Modules.Implementations.Help;

public class HelpMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Help";

    public int PagesCount { get; set; }

    protected override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(PagesCount)] = PagesCount.ToString();
    }

    protected override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        var pagesCount = 0;

        var success = values.TryGetValue(nameof(PagesCount), out var pagesCountData) && int.TryParse(pagesCountData, out pagesCount);

        if (!success)
            return false;

        PagesCount = pagesCount;
        return true;
    }

    protected override void Reset()
    {
        PagesCount = default;
    }
}
