using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.Emote;

namespace GrillBot.App.Actions.Commands.Emotes;

/// <summary>
/// Get detailed statistics about emote.
/// </summary>
public class EmoteInfo : CommandAction
{
    private ITextsManager Texts { get; }
    private FormatHelper FormatHelper { get; }

    public string? ErrorMessage { get; private set; }
    public bool IsOk => string.IsNullOrEmpty(ErrorMessage);

    private readonly IServiceClientExecutor<IEmoteServiceClient> _emoteServiceClient;
    private readonly DataResolveManager _dataResolve;

    public EmoteInfo(ITextsManager texts, FormatHelper formatHelper, IServiceClientExecutor<IEmoteServiceClient> emoteServiceClient, DataResolveManager dataResolve)
    {
        Texts = texts;
        FormatHelper = formatHelper;
        _emoteServiceClient = emoteServiceClient;
        _dataResolve = dataResolve;
    }

    public async Task<Embed?> ProcessAsync(IEmote emoteData)
    {
        if (!CheckEmote(emoteData, out var emote))
            return null;

        var guildId = Context.Guild.Id.ToString();
        var emoteInfo = await _emoteServiceClient.ExecuteRequestAsync((c, cancellationToken) => c.GetEmoteInfoAsync(guildId, emote!.ToString(), cancellationToken));

        return await CreateEmbedAsync(emoteInfo);
    }

    private bool CheckEmote(IEmote emote, out Emote? result)
    {
        result = emote as Emote;
        if (result == null)
            ErrorMessage = Texts["Emote/Info/NotSupported", Locale];
        return IsOk;
    }

    private async Task<List<string>> GetFormattedTopUsersAsync(Dictionary<string, long> topUsers)
    {
        var result = new List<string>(topUsers.Count);
        var unknownUserTemplate = Texts["Emote/Info/UnknownUser", Locale];
        var rowTemplate = Texts["Emote/Info/Row", Locale];

        for (var i = 0; i < topUsers.Count; i++)
        {
            var item = topUsers.ElementAt(i);
            var user = await _dataResolve.GetUserAsync(item.Key.ToUlong());
            var username = (user?.GlobalAlias ?? user?.Username) ?? unknownUserTemplate;
            var row = rowTemplate.FormatWith($"{i + 1,2}", username, item.Value.FormatNumber());

            result.Add(row);
        }

        return result;
    }

    private async Task<Embed> CreateEmbedAsync(Core.Services.Emote.Models.Response.EmoteInfo emoteInfo)
    {
        var embed = new EmbedBuilder()
            .WithFooter(Context.User)
            .WithAuthor(Texts["Emote/Info/Embed/Title", Locale])
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .AddField(Texts["Emote/Info/Embed/Fields/Name", Locale], emoteInfo.EmoteName, true)
            .AddField(Texts["Emote/Info/Embed/Fields/Animated", Locale], FormatHelper.FormatBoolean("Emote/Info/Embed/Boolean", Locale, emoteInfo.IsEmoteAnimated), true)
            .WithThumbnailUrl(emoteInfo.EmoteUrl)
            ;

        if (!string.IsNullOrEmpty(emoteInfo.OwnerGuildId))
        {
            var guild = await _dataResolve.GetGuildAsync(emoteInfo.OwnerGuildId.ToUlong());
            embed.AddField(Texts["Emote/Info/Embed/Fields/Guild", Locale], guild?.Name ?? Texts["Emote/Info/UnknownGuild", Locale], true);
        }

        if (emoteInfo.Statistics is not null)
        {
            var topTen = await GetFormattedTopUsersAsync(emoteInfo.Statistics.TopUsers);

            embed
                .AddField(Texts["Emote/Info/Embed/Fields/FirstOccurence", Locale], emoteInfo.Statistics.FirstOccurenceUtc.ToTimestampMention(), true)
                .AddField(Texts["Emote/Info/Embed/Fields/LastOccurence", Locale], emoteInfo.Statistics.LastOccurenceUtc.ToTimestampMention(), true)
                .AddField(Texts["Emote/Info/Embed/Fields/UseCount", Locale], emoteInfo.Statistics.UseCount.FormatNumber(), true)
                .AddField(Texts["Emote/Info/Embed/Fields/UsedUsers", Locale], emoteInfo.Statistics.UsersCount.FormatNumber(), true)
                .AddField(Texts["Emote/Info/Embed/Fields/TopTen", Locale], string.Join("\n", topTen));
        }

        return embed
            .AddField(Texts["Emote/Info/Embed/Fields/Link", Locale], emoteInfo.EmoteUrl)
            .Build();
    }
}
