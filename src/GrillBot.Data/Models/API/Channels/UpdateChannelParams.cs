using System.Collections.Generic;
using GrillBot.Common.Infrastructure;

namespace GrillBot.Data.Models.API.Channels;

public class UpdateChannelParams : IApiObject
{
    public long Flags { get; set; }

    public Dictionary<string, string> SerializeForLog()
    {
        return new Dictionary<string, string>
        {
            { nameof(Flags), Flags.ToString() }
        };
    }
}
