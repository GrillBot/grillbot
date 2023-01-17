using System.Diagnostics.CodeAnalysis;
using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Searching;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("search", "Searching")]
[ExcludeFromCodeCoverage]
public class SearchingModule : InteractionsModuleBase
{
    public SearchingModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("list", "Current search.")]
    public async Task SearchingListAsync(
        [Summary("channel", "The channel you want to find something in.")]
        ITextChannel? channel = null,
        [Summary("substring", "Search substring")] [Discord.Interactions.MaxLength(50)]
        string? query = null
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
        var handler = new SearchingPaginationHandler(ServiceProvider!, page);
        await handler.ProcessAsync(Context);
    }

    [SlashCommand("create", "Create a new search.")]
    public async Task CreateSearchAsync(
        [Summary("message", "Message")] [Discord.Interactions.MaxLength(EmbedFieldBuilder.MaxFieldValueLength - 3)]
        string message
    )
    {
        using var command = GetCommand<Actions.Commands.Searching.CreateSearch>();

        await command.Command.ProcessAsync(message);
        await SetResponseAsync(GetText(nameof(CreateSearchAsync), "Success"));
    }

    [SlashCommand("remove", "Deletes the search")]
    public async Task RemoveSearchAsync(
        [Autocomplete(typeof(SearchingAutoCompleteHandler))] [Summary("ident", "Search identification")]
        long ident
    )
    {
        using var command = GetCommand<Actions.Commands.Searching.RemoveSearch>();

        await command.Command.ProcessAsync(ident);
        if (!string.IsNullOrEmpty(command.Command.ErrorMessage))
            await SetResponseAsync(command.Command.ErrorMessage);
        else
            await SetResponseAsync(GetText(nameof(RemoveSearchAsync), "Success"));
    }
}
