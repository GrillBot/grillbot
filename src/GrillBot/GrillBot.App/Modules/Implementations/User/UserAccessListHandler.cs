using GrillBot.App.Infrastructure;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;

namespace GrillBot.App.Modules.Implementations.User;

public class UserAccessListHandler : ComponentInteractionHandler
{
    private IDiscordClient DiscordClient { get; }
    private int Page { get; }

    public UserAccessListHandler(IDiscordClient discordClient, int page)
    {
        DiscordClient = discordClient;
        Page = page;
    }

    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!TryParseData<UserAccessListMetadata>(context.Interaction, out var component, out var metadata))
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

        var forUser = await guild.GetUserAsync(metadata.ForUserId);
        if (forUser == null)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var visibleChannels = await guild.GetAvailableChannelsAsync(forUser);
        visibleChannels = visibleChannels.FindAll(o => o is not ICategoryChannel);

        // We calculate a new view to get PagesCount without knowing if this is possible. Because we need to get the number of pages.
        // After this operation we will generate another embed because the page can still change.
        new EmbedBuilder().WithUserAccessList(visibleChannels, forUser, context.User, guild, 0, out var pagesCount);

        var newPage = CheckNewPageNumber(Page, pagesCount);
        if (newPage == metadata.Page)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var result = new EmbedBuilder()
            .WithUserAccessList(visibleChannels, forUser, context.User, guild, newPage, out pagesCount);

        await component.UpdateAsync(msg =>
        {
            msg.Components = ComponentsHelper.CreatePaginationComponents(newPage, pagesCount, "user_access");
            msg.Embed = result.Build();
        });
    }
}
