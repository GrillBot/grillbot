using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Searching;
using SearchingService.Models.Events;

namespace GrillBot.App.Modules.Interactions;

[RequireUserPerms]
[Group("search", "Searching")]
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
        using var command = await GetCommandAsync<Actions.Commands.Searching.GetSearchingList>();
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
        [Summary("message", "Message")][Discord.Interactions.MaxLength(EmbedFieldBuilder.MaxFieldValueLength - 3)] string message,
        [Summary("validTo", "ValidTo")] DateTime? validTo = null
    )
    {
        await SendViaRabbitAsync(new SearchItemPayload(User, Guild, Channel, message, validTo?.ToUniversalTime()));
        await SetResponseAsync(GetText(nameof(CreateSearchAsync), "Success"));
    }

    [SlashCommand("remove", "Deletes the search")]
    public async Task RemoveSearchAsync(
        [Autocomplete(typeof(SearchingAutoCompleteHandler))] [Summary("ident", "Search identification")]
        long ident
    )
    {
        using var command = await GetCommandAsync<Actions.Commands.Searching.RemoveSearch>();

        await command.Command.ProcessAsync(ident);
        if (!string.IsNullOrEmpty(command.Command.ErrorMessage))
            await SetResponseAsync(command.Command.ErrorMessage);
        else
            await SetResponseAsync(GetText(nameof(RemoveSearchAsync), "Success"));
    }
}
