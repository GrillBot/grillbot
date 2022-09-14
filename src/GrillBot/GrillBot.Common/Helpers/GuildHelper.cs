using Discord;
using GrillBot.Common.Managers;

namespace GrillBot.Common.Helpers;

public class GuildHelper
{
    private LocalizationManager Localization { get; }

    public GuildHelper(LocalizationManager localization)
    {
        Localization = localization;
    }

    public IEnumerable<string> GetFeatures(IGuild guild, string locale, string localeId)
    {
        if (guild.Features.Value == GuildFeature.None)
            return Enumerable.Empty<string>();

        return Enum.GetValues<GuildFeature>()
            .Where(o => o > 0 && guild.Features.HasFeature(o))
            .Select(o => Localization.Get($"{localeId}/{o}", locale))
            .Where(o => !string.IsNullOrEmpty(o))
            .Distinct()
            .OrderBy(o => o);
    }
}
