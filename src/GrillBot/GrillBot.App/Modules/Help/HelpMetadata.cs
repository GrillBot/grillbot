using GrillBot.App.Infrastructure.Embeds;
using System.Collections.Generic;

namespace GrillBot.App.Modules.Help
{
    public class HelpMetadata : IEmbedMetadata
    {
        public string EmbedKind => "Help";

        public int Page { get; set; }
        public int PagesCount { get; set; }

        public void SaveInto(IDictionary<string, string> destination)
        {
            destination[nameof(Page)] = Page.ToString();
            destination[nameof(PagesCount)] = PagesCount.ToString();
        }

        public bool TryLoadFrom(IReadOnlyDictionary<string, string> values)
        {
            int page = 0;
            int pagesCount = 0;

            var success = values.TryGetValue(nameof(Page), out string _page) && int.TryParse(_page, out page);
            success &= values.TryGetValue(nameof(PagesCount), out string _pagesCount) && int.TryParse(_pagesCount, out pagesCount);

            if(success)
            {
                Page = page;
                PagesCount = pagesCount;
                return true;
            }

            return false;
        }
    }
}
