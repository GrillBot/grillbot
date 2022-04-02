using Discord.Interactions;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Searching;
using GrillBot.App.Services;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("hledam", "Hledání")]
public class SearchingModule : Infrastructure.InteractionsModuleBase
{
    private SearchingService SearchingService { get; }

    public SearchingModule(SearchingService searchingService)
    {
        SearchingService = searchingService;
    }

    [SlashCommand("list", "Aktuální hledání.")]
    public async Task SearchingListAsync(
        [Summary("kanal", "Kanál, ve kterém chcete něco najít.")]
        ITextChannel channel = null,
        [Summary("podretezec", "Vyhledávací podřetězec")]
        string query = null
    )
    {
        if (channel == null) channel = (ITextChannel)Context.Channel;

        var list = await SearchingService.GetSearchListAsync(Context.Guild, channel, query, 0);
        var count = await SearchingService.GetItemsCountAsync(Context.Guild, channel, query);
        var pagesCount = (int)Math.Ceiling(count / (double)EmbedBuilder.MaxFieldCount);

        var embed = new EmbedBuilder()
            .WithSearching(list, channel, Context.Guild, 0, Context.User, query);

        var components = ComponentsHelper.CreatePaginationComponents(0, pagesCount, "search");
        await SetResponseAsync(embed: embed.Build(), components: components);
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("search:*", ignoreGroupNames: true)]
    public async Task HandleRemindListPaginationAsync(int page)
    {
        var handler = new SearchingPaginationHandler(SearchingService, Context.Client, page);
        await handler.ProcessAsync(Context);
    }

    [SlashCommand("nove", "Vytvoření nového hledání.")]
    public async Task CreateSearchAsync(
        [Summary("zprava", "Zpráva")]
        string message
    )
    {
        try
        {
            await SearchingService.CreateAsync(Context.Guild, Context.User, Context.Channel, message);
            await SetResponseAsync("Hledání bylo úspěšně uloženo.");
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [SlashCommand("smazat", "Smaže hledání")]
    public async Task RemoveRemindAsync(
        [Autocomplete(typeof(SearchingAutoCompleteHandler))]
        [Summary("ident", "Identifikace hledání")]
        long ident
    )
    {
        try
        {
            await SearchingService.RemoveSearchAsync(ident, Context.User as IGuildUser);
            await SetResponseAsync("Hledání bylo úspěšně smazáno.");
        }
        catch (UnauthorizedAccessException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }
}
