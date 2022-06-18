using Discord.Interactions;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Emotes;
using GrillBot.App.Services.Emotes;
using GrillBot.Common.Helpers;

namespace GrillBot.App.Modules.Interactions;

[Group("emote", "Správa emotů")]
[RequireUserPerms]
public class EmoteModule : Infrastructure.InteractionsModuleBase
{
    private EmotesCommandService EmotesCommandService { get; }

    public EmoteModule(EmotesCommandService emotesCommandService)
    {
        EmotesCommandService = emotesCommandService;
    }

    [SlashCommand("get", "Informace o emote")]
    public async Task GetEmoteInfoAsync(
        [Summary("emote", "Identifikace emote (ID/Název/Celý emote)")] IEmote emote
    )
    {
        try
        {
            var result = await EmotesCommandService.GetInfoAsync(emote, Context.User);
            if (result == null)
                await SetResponseAsync("U tohoto emote ještě nebyla zaznamenána žádná aktivita.");
            else
                await SetResponseAsync(embed: result);
        }
        catch (ArgumentException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [SlashCommand("list", "Seznam statistik emotů")]
    public async Task GetEmoteStatsListAsync(
        [Summary("order", "Seřadit podle")]
        [Choice("Počtu použití", "UseCount")]
        [Choice("Data a času posledního použití", "LastOccurence")]
        string orderBy,
        [Summary("direction", "Vzestupně/Sestupně")]
        [Choice("Sestupně", "true")]
        [Choice("Vzestupně", "false")]
        bool descending,
        [Summary("user", "Zobrazit statistiku pouze jednoho uživatele")]
        IUser ofUser = null,
        [Summary("animovane", "Chci v seznamu zobrazit i animované emoty?")]
        [Choice("Zobrazit animované emoty.", "false")]
        [Choice("Pryč animované emoty.", "true")]
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
