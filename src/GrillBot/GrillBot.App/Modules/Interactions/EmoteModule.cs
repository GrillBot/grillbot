using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Emotes;
using GrillBot.App.Services.Emotes;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Modules.Interactions;

[Group("emote", "Managing emotes")]
[RequireUserPerms]
public class EmoteModule : InteractionsModuleBase
{
    private EmotesCommandService EmotesCommandService { get; }

    public EmoteModule(EmotesCommandService emotesCommandService, ITextsManager texts) : base(texts)
    {
        EmotesCommandService = emotesCommandService;
    }

    [SlashCommand("get", "Emote information")]
    public async Task GetEmoteInfoAsync(
        [Summary("emote", "Emote identification (ID/Name/Full emote)")]
        IEmote emote
    )
    {
        try
        {
            var result = await EmotesCommandService.GetInfoAsync(emote, Context.User);
            if (result == null)
                await SetResponseAsync(GetText(nameof(GetEmoteInfoAsync), "NoActivity"));
            else
                await SetResponseAsync(embed: result);
        }
        catch (ArgumentException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [SlashCommand("list", "List of emote stats")]
    public async Task GetEmoteStatsListAsync(
        [Summary("order", "Sort by")] [Choice("Number of uses", "UseCount")] [Choice("Date and time of last use", "LastOccurence")]
        string orderBy,
        [Summary("direction", "Ascending/Descending")] [Choice("Ascending", "true")] [Choice("Descending", "false")]
        bool descending,
        [Summary("user", "Show statistics of only one user")]
        IUser ofUser = null,
        [Summary("animated", "Do I want to show animated emotes in the list too?")] [Choice("Show animated emotes", "false")] [Choice("Hide animated emotes", "true")]
        bool filterAnimated = false
    )
    {
        var result = await EmotesCommandService.GetEmoteStatListEmbedAsync(Context, ofUser, orderBy, descending, filterAnimated);
        var pagesCount = (int)Math.Ceiling(result.Item2 / ((double)EmbedBuilder.MaxFieldCount - 1));

        var components = ComponentsHelper.CreatePaginationComponents(1, pagesCount, "emote");
        await SetResponseAsync(embed: result.Item1, components: components);
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("emote:*", ignoreGroupNames: true)]
    public async Task HandleEmoteListPaginationAsync(int page)
    {
        var handler = new EmotesListPaginationHandler(EmotesCommandService, Context.Client, page);
        await handler.ProcessAsync(Context);
    }
}
