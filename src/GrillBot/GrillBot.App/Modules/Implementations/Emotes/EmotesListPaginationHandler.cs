using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure;
using GrillBot.App.Services.Emotes;

namespace GrillBot.App.Modules.Implementations.Emotes;

public class EmotesListPaginationHandler : ComponentInteractionHandler
{
    private int Page { get; }
    private EmotesCommandService EmotesCommandService { get; }
    private IDiscordClient DiscordClient { get; }

    public EmotesListPaginationHandler(EmotesCommandService emotesCommandService, IDiscordClient discordClient, int page)
    {
        EmotesCommandService = emotesCommandService;
        DiscordClient = discordClient;
        Page = page;
    }

    public override async Task ProcessAsync(IInteractionContext context)
    {
        if (!TryParseData<EmoteListMetadata>(context.Interaction, out var component, out var metadata))
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

        var ofUser = metadata.OfUserId == null ? null : await DiscordClient.FindUserAsync(metadata.OfUserId.Value);

        var count = await EmotesCommandService.GetEmoteStatsCountAsync(context, ofUser, metadata.FilterAnimated);
        var pagesCount = (int)Math.Ceiling(count / ((double)EmbedBuilder.MaxFieldCount - 1));
        var newPage = CheckNewPageNumber(Page, pagesCount);
        if (newPage + 1 == metadata.Page)
        {
            await context.Interaction.DeferAsync();
            return;
        }

        var result = await EmotesCommandService.GetEmoteStatListEmbedAsync(context, ofUser, metadata.OrderBy, metadata.Descending,
            metadata.FilterAnimated, newPage + 1);

        await component.UpdateAsync(msg =>
        {
            msg.Components = ComponentsHelper.CreatePaginationComponents(newPage, pagesCount, "emote");
            msg.Embed = result.Item1;
        });
    }
}
