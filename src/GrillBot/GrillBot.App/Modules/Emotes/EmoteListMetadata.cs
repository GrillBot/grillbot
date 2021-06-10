using GrillBot.App.Infrastructure.Embeds;
using System.Collections.Generic;

namespace GrillBot.App.Modules.Emotes
{
    public class EmoteListMetadata : IEmbedMetadata
    {
        public string EmbedKind => "EmoteList";

        public bool IsPrivate { get; set; }
        public int Page { get; set; }
        public bool Desc { get; set; }
        public string SortBy { get; set; }
        public ulong? OfUserId { get; set; }

        public void SaveInto(IDictionary<string, string> destination)
        {
            destination[nameof(IsPrivate)] = IsPrivate.ToString();
            destination[nameof(Page)] = Page.ToString();
            destination[nameof(Desc)] = Desc.ToString();
            destination[nameof(SortBy)] = SortBy;

            if (OfUserId != null)
                destination[nameof(OfUserId)] = OfUserId.Value.ToString();
        }

        public bool TryLoadFrom(IReadOnlyDictionary<string, string> values)
        {
            bool isPrivate = false;
            int page = 0;
            bool desc = false;
            ulong ofUserId = 0;

            var success = values.TryGetValue(nameof(IsPrivate), out var _isPrivate) && bool.TryParse(_isPrivate, out isPrivate);
            success &= values.TryGetValue(nameof(Page), out var _page) && int.TryParse(_page, out page);
            success &= values.TryGetValue(nameof(Desc), out var _desc) && bool.TryParse(_desc, out desc);
            success &= values.TryGetValue(nameof(SortBy), out string sortBy);
            success &= !values.TryGetValue(nameof(OfUserId), out var _ofUserId) || ulong.TryParse(_ofUserId, out ofUserId);

            if (success)
            {
                IsPrivate = isPrivate;
                Page = page;
                Desc = desc;
                SortBy = sortBy;
                OfUserId = ofUserId == 0 ? null : ofUserId;
                return true;
            }

            return false;
        }
    }
}
