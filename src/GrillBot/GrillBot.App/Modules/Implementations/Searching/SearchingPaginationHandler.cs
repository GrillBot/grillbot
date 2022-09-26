using GrillBot.App.Infrastructure;
using GrillBot.Common.Helpers;
using GrillBot.Data.Models.API.Searching;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Modules.Implementations.Searching;

public class SearchingPaginationHandler : ComponentInteractionHandler
{
    private int Page { get; }
    private IDiscordClient DiscordClient { get; }
    private IServiceProvider ServiceProvider { get; }

    public SearchingPaginationHandler(IDiscordClient discordClient, IServiceProvider serviceProvider, int page)
    {
        DiscordClient = discordClient;
        Page = page;
        ServiceProvider = serviceProvider;
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

        var parameters = new GetSearchingListParams
        {
            Pagination = { Page = 0, PageSize = EmbedBuilder.MaxFieldCount },
            Sort = { Descending = false, OrderBy = "Id" },
            ChannelId = channel.Id.ToString(),
            GuildId = guild.Id.ToString(),
            MessageQuery = metadata.MessageQuery
        };

        using var scope = ServiceProvider.CreateScope();

        var databaseBuilder = scope.ServiceProvider.GetRequiredService<GrillBotDatabaseBuilder>();
        await using var repository = databaseBuilder.CreateRepository();

        var searchesCount = await repository.Searching.GetSearchesCountAsync(parameters, new List<string>());
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

        var action = scope.ServiceProvider.GetRequiredService<Actions.Api.V1.Searching.GetSearchingList>();
        parameters.Pagination.Page = newPage;
        var searches = await action.ProcessAsync(parameters);
        var result = new EmbedBuilder()
            .WithSearching(searches, channel, guild, newPage, context.User, metadata.MessageQuery);

        await component.UpdateAsync(msg =>
        {
            msg.Components = ComponentsHelper.CreatePaginationComponents(newPage, pagesCount, "search");
            msg.Embed = result.Build();
        });
    }
}
