using System.Linq;
using System.Reflection;
using Discord;

namespace GrillBot.Tests.TestHelpers;

public class EmoteHelper
{
    public static GuildEmote CreateGuildEmote(Emote emote)
    {
        var constructor = typeof(GuildEmote)
            .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault();
        if (constructor == null) return null;

        var instance = constructor.Invoke(new object[] { emote.Id, emote.Name, emote.Animated, false, false, new List<ulong>(), null });
        return (GuildEmote)instance;
    }
}
