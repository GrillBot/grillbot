using Discord;
using GrillBot.Common.Managers.Emotes;

namespace GrillBot.Tests.Infrastructure;

public class EmotesCacheBuilder : BuilderBase<IEmoteCache>
{
    private List<CachedEmote> Emotes { get; } = new();

    public EmotesCacheBuilder AddEmote(GuildEmote emote, IGuild guild)
    {
        Emotes.Add(new CachedEmote { Emote = emote, Guild = guild });
        return this;
    }

    public override IEmoteCache Build()
    {
        Mock.Setup(o => o.GetEmotes()).Returns(Emotes);
        return base.Build();
    }
}
