using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Emotes;
using GrillBot.Data.Enums;

namespace GrillBot.App.Modules.Interactions;

[Group("emote", "Managing emotes")]
[RequireUserPerms]
public class EmoteModule : InteractionsModuleBase
{
    public EmoteModule(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [SlashCommand("get", "Emote information")]
    public async Task GetEmoteInfoAsync(
        [Summary("emote", "Emote identification (ID/Name/Full emote)")]
        IEmote emote
    )
    {
        using var command = GetCommand<Actions.Commands.Emotes.EmoteInfo>();

        var result = await command.Command.ProcessAsync(emote);
        if (!command.Command.IsOk)
            await SetResponseAsync(command.Command.ErrorMessage);
        else
            await SetResponseAsync(embed: result);
    }

    [SlashCommand("list", "List of emote stats")]
    public async Task GetEmoteStatsListAsync(
        [Summary("order", "Sort by")] [Choice("Number of uses", "UseCount")] [Choice("Date and time of last use", "LastOccurence")]
        string orderBy,
        [Summary("sort", "Ascending/Descending")]
        SortType sort,
        [Summary("user", "Show statistics of only one user")]
        IUser? ofUser = null,
        [Summary("animated", "Do I want to show animated emotes in the list too?")] [Choice("Show animated emotes", "false")] [Choice("Hide animated emotes", "true")]
        bool filterAnimated = false
    )
    {
        using var command = GetCommand<Actions.Commands.Emotes.GetEmotesList>();
        var (embed, paginationComponent) = await command.Command.ProcessAsync(0, orderBy, sort, ofUser, filterAnimated);

        await SetResponseAsync(embed: embed, components: paginationComponent);
    }

    [RequireSameUserAsAuthor]
    [ComponentInteraction("emote:*", ignoreGroupNames: true)]
    public async Task HandleEmoteListPaginationAsync(int page)
    {
        var handler = new EmotesListPaginationHandler(ServiceProvider, page);
        await handler.ProcessAsync(Context);
    }
}
