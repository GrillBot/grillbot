using System.Collections.Generic;
using GrillBot.Core.Infrastructure;

namespace GrillBot.Data.Models.API.Channels;

public class UpdateChannelParams : IDictionaryObject
{
    public long Flags { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(Flags), Flags.ToString() }
        };
    }
}
