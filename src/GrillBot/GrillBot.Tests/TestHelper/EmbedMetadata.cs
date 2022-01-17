using GrillBot.Data.Infrastructure.Embeds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.TestHelper
{
    internal class EmbedMetadata : IEmbedMetadata
    {
        public string EmbedKind { get; } = "Embed";
        public bool LoadResult { get; set; }

        public virtual void SaveInto(IDictionary<string, string> destination)
        {
            destination["LoadResult"] = LoadResult.ToString();
        }

        public virtual bool TryLoadFrom(IReadOnlyDictionary<string, string> values)
        {
            LoadResult = Convert.ToBoolean(values["LoadResult"]);
            return LoadResult;
        }
    }
}
