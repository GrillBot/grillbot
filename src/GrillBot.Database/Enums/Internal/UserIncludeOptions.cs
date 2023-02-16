using System;

namespace GrillBot.Database.Enums.Internal;

[Flags]
public enum UserIncludeOptions
{
    None = 0,
    Guilds = 1,
    UsedInvite = 2,
    CreatedInvites = 4,
    Channels = 8,
    EmoteStatistics = 16,
    Unverify = 32,
    Nicknames = 64,
    
    All = Guilds | UsedInvite | CreatedInvites | Channels | EmoteStatistics | Unverify | Nicknames
}
