using Newtonsoft.Json;
using System.Collections.Generic;

namespace GrillBot.Data.Models
{
    public class Diff<TType>
    {
        public TType Before { get; set; }
        public TType After { get; set; }

        public Diff() { }

        public Diff(TType before, TType after)
        {
            Before = before;
            After = after;
        }

        [JsonIgnore]
        public bool IsEmpty => Comparer<TType>.Default.Compare(Before, After) == 0;
    }
}
