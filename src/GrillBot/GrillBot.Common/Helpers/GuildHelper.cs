using Discord;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.Common.Helpers;

public class GuildHelper
{
    private ITextsManager Texts { get; }

    public GuildHelper(ITextsManager texts)
    {
        Texts = texts;
    }

    public IEnumerable<string> GetFeatures(IGuild guild, string locale, string localeId)
    {
        if (guild.Features.Value == GuildFeature.None)
            return Enumerable.Empty<string>();

        return Enum.GetValues<GuildFeature>()
            .Where(o => o > 0 && guild.Features.HasFeature(o))
            .Select(o => Texts[$"{localeId}/{o}", locale])
            .Where(o => !string.IsNullOrEmpty(o))
            .Distinct()
            .OrderBy(o => o);
    }
}
