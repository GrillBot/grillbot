using System;

namespace GrillBot.Data.Models.API.Unverify
{
    public class UnverifyLogUpdate
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public UnverifyLogUpdate() { }

        public UnverifyLogUpdate(Models.Unverify.UnverifyLogUpdate entity)
        {
            Start = entity.Start;
            End = entity.End;
        }
    }
}
