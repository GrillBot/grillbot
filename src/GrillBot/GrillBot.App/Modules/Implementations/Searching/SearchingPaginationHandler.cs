using GrillBot.App.Infrastructure;
using GrillBot.App.Services;

namespace GrillBot.App.Modules.Implementations.Searching;

public class SearchingPaginationHandler : ComponentInteractionHandler
{
    private int Page { get; }
    private SearchingService SearchingService { get; }
    private IDiscordClient DiscordClient { get; }

    public SearchingPaginationHandler(SearchingService searchingService, IDiscordClient discordClient, int page)
    {
        SearchingService = searchingService;
        DiscordClient = discordClient;
        Page = page;
    }

    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!TryParseData<SearchingMetadata>(context.Interaction, out var component, out var metadata))
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var guild = await DiscordClient.GetGuildAsync(metadata.GuildId);
        if (guild == null)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var channel = await guild.GetTextChannelAsync(metadata.ChannelId);
        if (channel == null)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var searchesCount = await SearchingService.GetItemsCountAsync(guild, channel, metadata.MessageQuery);
        if (searchesCount == 0)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var pagesCount = (int)Math.Ceiling(searchesCount / (double)EmbedBuilder.MaxFieldCount);
        var newPage = CheckNewPageNumber(Page, pagesCount);
        if (newPage == metadata.Page)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var searches = await SearchingService.GetSearchListAsync(guild, channel, metadata.MessageQuery, newPage);
        var result = new EmbedBuilder()
            .WithSearching(searches, channel, guild, newPage, context.User, metadata.MessageQuery);

        await component.UpdateAsync(msg => msg.Embed = result.Build());
    }
}
