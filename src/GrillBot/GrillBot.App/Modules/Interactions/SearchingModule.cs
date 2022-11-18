using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Searching;
using GrillBot.App.Services;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("search", "Searching")]
[ExcludeFromCodeCoverage]
public class SearchingModule : InteractionsModuleBase
{
    private SearchingService SearchingService { get; }

    public SearchingModule(SearchingService searchingService, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        SearchingService = searchingService;
    }

    [SlashCommand("list", "Current search.")]
    public async Task SearchingListAsync(
        [Summary("channel", "The channel you want to find something in.")]
        ITextChannel channel = null,
        [Summary("substring", "Search substring")] [Discord.Interactions.MaxLength(50)]
        string query = null
    )
    {
        using var command = GetCommand<Actions.Commands.Searching.GetSearchingList>();
        var (embed, paginationComponent) = await command.Command.ProcessAsync(0, query, channel);

        await SetResponseAsync(embed: embed, components: paginationComponent);
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("search:*", ignoreGroupNames: true)]
    public async Task HandleSearchingListPaginationAsync(int page)
    {
        var handler = new SearchingPaginationHandler(ServiceProvider, page);
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
            await SetResponseAsync(GetText(nameof(CreateSearchAsync), "Success"));
        }
        catch (ValidationException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }

    [SlashCommand("remove", "Deletes the search")]
    public async Task RemoveSearchAsync(
        [Autocomplete(typeof(SearchingAutoCompleteHandler))] [Summary("ident", "Search identification")]
        long ident
    )
    {
        try
        {
            await SearchingService.RemoveSearchAsync(ident, Context.User as IGuildUser);
            await SetResponseAsync(GetText(nameof(RemoveSearchAsync), "Success"));
        }
        catch (UnauthorizedAccessException ex)
        {
            await SetResponseAsync(ex.Message);
        }
    }
}
