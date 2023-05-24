using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Database.Entity;
using GrillBot.Database.Models.Emotes;

namespace GrillBot.App.Actions.Commands.Emotes;

/// <summary>
/// Get detailed statistics about emote.
/// </summary>
public class EmoteInfo : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private ITextsManager Texts { get; }
    private FormatHelper FormatHelper { get; }

    public string? ErrorMessage { get; private set; }
    public bool IsOk => string.IsNullOrEmpty(ErrorMessage);

    public EmoteInfo(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient, ITextsManager texts, FormatHelper formatHelper)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        Texts = texts;
        FormatHelper = formatHelper;
    }

    public async Task<Embed?> ProcessAsync(IEmote emoteData)
    {
        if (!CheckEmote(emoteData, out var emote)) return null;

        var statsData = await GetStatisticsAsync(emote!);
        if (statsData == null) return null;
        var (statistics, topTen) = statsData.Value;

        var guild = await DiscordClient.GetGuildAsync(statistics.GuildId.ToUlong());
        var topTenData = await GetFormattedTopTenAsync(topTen);
        return CreateEmbed(emote!, statistics, guild, topTenData);
    }

    private bool CheckEmote(IEmote emote, out Emote? result)
    {
        result = emote as Emote;
        if (result == null)
            ErrorMessage = Texts["Emote/Info/NotSupported", Locale];
        return IsOk;
    }

    private async Task<(EmoteStatItem statistics, List<EmoteStatisticItem> topTen)?> GetStatisticsAsync(IEmote emote)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var statictics = await repository.Emote.GetStatisticsOfEmoteAsync(emote);
        if (statictics == null)
        {
            ErrorMessage = Texts["Emote/Info/NoActivity", Locale];
            return null;
        }

        var topTen = await repository.Emote.GetTopUsersOfUsage(emote, 10);
        return (statictics, topTen);
    }

    private async Task<List<string>> GetFormattedTopTenAsync(IReadOnlyList<EmoteStatisticItem> statistics)
    {
        var count = Math.Min(10, statistics.Count);
        var result = new List<string>(count);
        var rowTemplate = Texts["Emote/Info/Row", Locale];
        var unknownUserTemplate = Texts["Emote/Info/UnknownUser", Locale];

        for (var i = 0; i < count; i++)
        {
            var stat = statistics[i];
            var user = await DiscordClient.FindUserAsync(stat.UserId.ToUlong());
            var username = user?.GetDisplayName() ?? unknownUserTemplate;
            var row = rowTemplate.FormatWith($"{i + 1,2}", username, stat.UseCount.FormatNumber());
            result.Add(row);
        }

        return result;
    }

    private Embed CreateEmbed(Emote emote, EmoteStatItem statistics, IGuild? guild, IEnumerable<string> topTen)
    {
        var fromLastUse = (DateTime.Now - statistics.LastOccurence).Humanize(culture: Texts.GetCulture(Locale));

        return new EmbedBuilder()
            .WithFooter(Context.User)
            .WithAuthor(Texts["Emote/Info/Embed/Title", Locale])
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .AddField(Texts["Emote/Info/Embed/Fields/Name", Locale], emote.Name, true)
            .AddField(Texts["Emote/Info/Embed/Fields/Animated", Locale], FormatHelper.FormatBoolean("Emote/Info/Embed/Boolean", Locale, emote.Animated), true)
            .AddField(Texts["Emote/Info/Embed/Fields/FirstOccurence", Locale], statistics.FirstOccurence.ToTimestampMention(), true)
            .AddField(Texts["Emote/Info/Embed/Fields/LastOccurence", Locale], statistics.LastOccurence.ToTimestampMention(), true)
            .AddField(Texts["Emote/Info/Embed/Fields/FromLastUse", Locale], fromLastUse, true)
            .AddField(Texts["Emote/Info/Embed/Fields/UseCount", Locale], statistics.UseCount.FormatNumber(), true)
            .AddField(Texts["Emote/Info/Embed/Fields/UsedUsers", Locale], statistics.UsedUsersCount.FormatNumber(), true)
            .AddField(Texts["Emote/Info/Embed/Fields/Guild", Locale], guild?.Name ?? Texts["Emote/Info/UnknownGuild", Locale], true)
            .AddField(Texts["Emote/Info/Embed/Fields/TopTen", Locale], string.Join("\n", topTen))
            .AddField(Texts["Emote/Info/Embed/Fields/Link", Locale], emote.Url)
            .WithThumbnailUrl(emote.Url)
            .Build();
    }
}
