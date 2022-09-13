using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Searching;
using GrillBot.App.Services;
using GrillBot.Common.Helpers;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("search", "Searching")]
public class SearchingModule : InteractionsModuleBase
{
    private SearchingService SearchingService { get; }

    public SearchingModule(SearchingService searchingService)
    {
        SearchingService = searchingService;
    }

    [SlashCommand("list", "Current search.")]
    public async Task SearchingListAsync(
        [Summary("channel", "The channel you want to find something in.")]
        ITextChannel channel = null,
        [Summary("substring", "Search substring")]
        [Discord.Interactions.MaxLength(50)]
        string query = null
    )
    {
        channel ??= (ITextChannel)Context.Channel;

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
    public async Task HandleSearchingListPaginationAsync(int page)
    {
        var handler = new SearchingPaginationHandler(SearchingService, Context.Client, page);
        await handler.ProcessAsync(Context);
    }

    [SlashCommand("create", "Create a new search.")]
    public async Task CreateSearchAsync(
        [Summary("message", "Message")] string message
    )
    {
        try
        {
            await SearchingService.CreateAsync(Context.Guild, Context.User as IGuildUser, Context.Channel as IGuildChannel, message);
            await SetResponseAsync("Hledání bylo úspěšně uloženo.");
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [SlashCommand("remove", "Deletes the search")]
    public async Task RemoveRemindAsync(
        [Autocomplete(typeof(SearchingAutoCompleteHandler))] [Summary("ident", "Search identification")]
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
