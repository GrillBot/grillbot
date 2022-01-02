using GrillBot.App.Infrastructure.Embeds;
using System.Collections.Generic;

namespace GrillBot.App.Modules.Implementations.Help;

public class HelpMetadata : PaginatedMetadataBase
{
    public override string EmbedKind => "Help";

    public int PagesCount { get; set; }

    public override void Save(IDictionary<string, string> destination)
    {
        destination[nameof(PagesCount)] = PagesCount.ToString();
    }

    public override bool TryLoad(IReadOnlyDictionary<string, string> values)
    {
        int pagesCount = 0;

        var success = values.TryGetValue(nameof(PagesCount), out string _pagesCount) && int.TryParse(_pagesCount, out pagesCount);

        if (success)
        {
            PagesCount = pagesCount;
            return true;
        }

        return false;
    }

    public override void Reset()
    {
        PagesCount = default;
    }
}
