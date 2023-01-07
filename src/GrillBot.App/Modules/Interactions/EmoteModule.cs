using Discord.Interactions;
using GrillBot.App.Infrastructure.Commands;
using GrillBot.App.Infrastructure.Preconditions.Interactions;
using GrillBot.App.Modules.Implementations.Emotes;
using GrillBot.App.Services.Emotes;

namespace GrillBot.App.Modules.Interactions;

[Group("emote", "Managing emotes")]
[RequireUserPerms]
public class EmoteModule : InteractionsModuleBase
{
    private EmotesCommandService EmotesCommandService { get; }

    public EmoteModule(EmotesCommandService emotesCommandService, IServiceProvider serviceProvider) : base(serviceProvider)
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
        [Summary("direction", "Ascending/Descending")] [Choice("Ascending", "false")] [Choice("Descending", "true")]
        bool descending,
        [Summary("user", "Show statistics of only one user")]
        IUser ofUser = null,
        [Summary("animated", "Do I want to show animated emotes in the list too?")] [Choice("Show animated emotes", "false")] [Choice("Hide animated emotes", "true")]
        bool filterAnimated = false
    )
    {
        using var command = GetCommand<Actions.Commands.Emotes.GetEmotesList>();
        var (embed, paginationComponent) = await command.Command.ProcessAsync(0, orderBy, descending, ofUser, filterAnimated);

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
