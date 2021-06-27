using GrillBot.App.Infrastructure.Embeds;
using Namotion.Reflection;
using System;
using System.Collections.Generic;

namespace GrillBot.App.Modules.Reminder
{
    public class RemindListMetadata : IEmbedMetadata
    {
        public string EmbedKind => "Reminder";

        public int Page { get; set; }
        public ulong OfUser { get; set; }

        public void SaveInto(IDictionary<string, string> destination)
        {
            destination[nameof(Page)] = Page.ToString();
            destination[nameof(OfUser)] = OfUser.ToString();
        }

        public bool TryLoadFrom(IReadOnlyDictionary<string, string> values)
        {
            int page = 0;
            ulong ofUser = 0;

            var success = values.TryGetValue(nameof(Page), out var _page) && int.TryParse(_page, out page);
            success &= values.TryGetValue(nameof(OfUser), out var _ofUser) && ulong.TryParse(_ofUser, out ofUser);

            if (success)
            {
                Page = page;
                OfUser = ofUser;
                return true;
            }

            return false;
        }
    }
}
