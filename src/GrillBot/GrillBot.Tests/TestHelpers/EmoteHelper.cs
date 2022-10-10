using Discord;

namespace GrillBot.Tests.TestHelpers;

public class EmoteHelper
{
    public static GuildEmote CreateGuildEmote(Emote emote)
    {
        var constructorParameters = new object[] { emote.Id, emote.Name, emote.Animated, false, false, new List<ulong>(), null };
        return ReflectionHelper.CreateWithInternalConstructor<GuildEmote>(constructorParameters);
    }
}
